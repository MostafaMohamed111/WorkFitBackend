using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Domain.Entities;
using WorkFit.CodeReview.Features.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Options;
using WorkFit.CodeReview.Infrastructure.Repositories;
using WorkFit.CodeReview.Infrastructure.Services.Models;
using WorkFit.Organizations.Contracts.OrganizationGitHub;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class CodeReviewWorkflowService : ICodeReviewWorkflowService
{
    private const string WorkflowName = "AI GitHub Code Review System";

    private readonly ICodeReviewRepository _repository;
    private readonly IGitHubCodeReviewService _gitHubService;
    private readonly ICodeReviewReviewerService _reviewerService;
    private readonly ICodeReviewAgentService _aiService;
    private readonly IOptions<CodeReviewOptions> _options;
    private readonly IGitHubOrganizationInstallationLookupService _organizationInstallationLookup;
    private readonly IGitHubAppAuthenticationService _appAuthService;
    private readonly ILogger<CodeReviewWorkflowService> _logger;

    public CodeReviewWorkflowService(
        ICodeReviewRepository repository,
        IGitHubCodeReviewService gitHubService,
        ICodeReviewReviewerService reviewerService,
        ICodeReviewAgentService aiService,
        IOptions<CodeReviewOptions> options,
        IGitHubOrganizationInstallationLookupService organizationInstallationLookup,
        IGitHubAppAuthenticationService appAuthService,
        ILogger<CodeReviewWorkflowService> logger)
    {
        _repository = repository;
        _gitHubService = gitHubService;
        _reviewerService = reviewerService;
        _aiService = aiService;
        _options = options;
        _organizationInstallationLookup = organizationInstallationLookup;
        _appAuthService = appAuthService;
        _logger = logger;
    }

    public async Task<CodeReviewWorkflowExecutionResult> ExecuteAsync(ReviewGitHubCommitCommand request, CancellationToken ct)
    {
        var executionId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var effectiveToken = await ResolveAccessTokenAsync(request.AccessToken, request.Organization, ct);
        var repoMetadata = await EnsureRepoMetadataAsync(request.Organization, request.Repository, effectiveToken, executionId, ct);

        var commit = await ExecuteStageAsync(
            "Fetch Commit",
            () => _gitHubService.GetCommitAsync(request.Organization, request.Repository, request.CommitSha, effectiveToken, ct),
            executionId,
            ct);

        return await ReviewFilesAsync(
            executionId,
            request.Organization,
            request.Repository,
            request.Branch,
            commit.Sha,
            request.PullRequestNumber,
            null,
            null,
            commit.Files,
            repoMetadata.DefaultBranch,
            now,
            ct,
            "No reviewable code files found in this commit after filtering.");
    }

    public async Task<CodeReviewWorkflowExecutionResult> ExecuteTaskAsync(ReviewTaskGitHubChangesCommand request, CancellationToken ct)
    {
        var executionId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var effectiveToken = await ResolveAccessTokenAsync(request.AccessToken, request.Organization, ct);
        var repoMetadata = await EnsureRepoMetadataAsync(request.Organization, request.Repository, effectiveToken, executionId, ct);

        var branch = request.Branch?.Trim();
        if (request.PullRequestNumber is null && string.IsNullOrWhiteSpace(branch))
        {
            throw new InvalidOperationException("Task GitHub branch is missing.");
        }

        string effectiveBranch = branch ?? string.Empty;
        string commitSha = string.Empty;
        IReadOnlyList<GitHubCommitFile> files;

        if (request.PullRequestNumber is not null)
        {
            var pullRequest = await ExecuteStageAsync(
                "Fetch Pull Request",
                () => _gitHubService.GetPullRequestAsync(request.Organization, request.Repository, request.PullRequestNumber.Value, effectiveToken, ct),
                executionId,
                ct,
                request.TaskId,
                request.EmployeeId);

            effectiveBranch = string.IsNullOrWhiteSpace(pullRequest.HeadBranch) ? effectiveBranch : pullRequest.HeadBranch;
            var baseBranch = string.IsNullOrWhiteSpace(pullRequest.BaseBranch) ? repoMetadata.DefaultBranch : pullRequest.BaseBranch;

            var comparison = await ExecuteStageAsync(
                "Fetch Pull Request Changes",
                () => _gitHubService.GetComparisonAsync(request.Organization, request.Repository, baseBranch, effectiveBranch, effectiveToken, ct),
                executionId,
                ct,
                request.TaskId,
                request.EmployeeId);

            commitSha = string.IsNullOrWhiteSpace(comparison.HeadSha) ? pullRequest.HeadSha : comparison.HeadSha;
            files = comparison.Files;
        }
        else
        {
            var comparison = await ExecuteStageAsync(
                "Fetch Task Changes",
                () => _gitHubService.GetComparisonAsync(request.Organization, request.Repository, repoMetadata.DefaultBranch, effectiveBranch, effectiveToken, ct),
                executionId,
                ct,
                request.TaskId,
                request.EmployeeId);

            commitSha = string.IsNullOrWhiteSpace(comparison.HeadSha) ? effectiveBranch : comparison.HeadSha;
            files = comparison.Files;
        }

        return await ReviewFilesAsync(
            executionId,
            request.Organization,
            request.Repository,
            effectiveBranch,
            commitSha,
            request.PullRequestNumber,
            request.TaskId,
            request.EmployeeId,
            files,
            repoMetadata.DefaultBranch,
            now,
            ct,
            "No reviewable code files found for this task after filtering.");
    }

    private async Task<GitHubRepositoryMetadata> EnsureRepoMetadataAsync(
        string organization,
        string repository,
        string? accessToken,
        string executionId,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cacheKey = $"{organization}/{repository}";

        var cached = await _repository.GetFreshRepoMetadataAsync(cacheKey, now, _options.Value.MetadataCacheTtl, ct);
        if (cached is not null)
        {
            return new GitHubRepositoryMetadata(0, cached.Repository, cached.DefaultBranch, cached.MetadataJson);
        }

        var repoMetadata = await ExecuteStageAsync(
            "Fetch Repo Metadata",
            () => _gitHubService.GetRepositoryMetadataAsync(organization, repository, accessToken, ct),
            executionId,
            ct);

        await _repository.UpsertRepoMetadataAsync(
            RepoMetadataCacheEntry.Create(
                cacheKey,
                organization,
                repository,
                repoMetadata.DefaultBranch,
                repoMetadata.RawJson,
                now),
            ct);

        await _repository.SaveChangesAsync(ct);
        return repoMetadata;
    }

    private async Task<string?> ResolveAccessTokenAsync(string? accessToken, string organization, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return accessToken.Trim();
        }

        var installationId = await _organizationInstallationLookup.GetGitHubInstallationIdForOrganizationAsync(organization, ct);
        if (installationId.HasValue)
        {
            return await _appAuthService.GetInstallationAccessTokenAsync(installationId.Value, ct);
        }

        return _options.Value.GitHub.PersonalAccessToken;
    }

    private async Task<CodeReviewWorkflowExecutionResult> ReviewFilesAsync(
        string executionId,
        string organization,
        string repository,
        string branch,
        string commitSha,
        int? pullRequestNumber,
        Guid? taskId,
        Guid? employeeId,
        IReadOnlyList<GitHubCommitFile> files,
        string defaultBranch,
        DateTime now,
        CancellationToken ct,
        string noFilesMessage)
    {
        var reviewableFiles = FilterReviewableFiles(files);
        var codeContext = BuildAiContext(reviewableFiles, out var truncated);

        if (string.IsNullOrWhiteSpace(codeContext))
        {
            var noFilesResponse = new CodeReviewResultDto(
                repository,
                string.IsNullOrWhiteSpace(commitSha) ? branch : commitSha,
                null,
                "Unknown",
                "Unknown",
                new Dictionary<string, int?>(),
                Array.Empty<string>(),
                Array.Empty<CodeReviewIssueDto>(),
                new[] { noFilesMessage },
                Array.Empty<string>());

            if (taskId.HasValue)
            {
                await _repository.AddSuccessLogAsync(
                    CodeReviewRunLogEntry.CreateSuccess(
                        executionId,
                        organization,
                        repository,
                        branch,
                        string.IsNullOrWhiteSpace(commitSha) ? branch : commitSha,
                        pullRequestNumber is null ? string.Empty : pullRequestNumber.Value.ToString(),
                        taskId,
                        employeeId,
                        0,
                        "Unknown",
                        noFilesMessage,
                        now),
                    ct);

                await _repository.SaveChangesAsync(ct);
            }

            return new CodeReviewWorkflowExecutionResult(executionId, noFilesResponse, string.Empty, string.Empty, false, truncated);
        }

        var reviewers = BuildReviewerConfigs();
        var reviewerResults = await ExecuteStageAsync(
            "Run Reviewer Agents",
            () => _reviewerService.RunReviewersAsync(reviewers, repository, commitSha, codeContext, ct),
            executionId,
            ct,
            taskId,
            employeeId);

        var aggregate = AggregateFindings(reviewerResults);
        var summaries = await ExecuteStageAsync(
            "Generate Summaries",
            () => _aiService.GenerateSummariesAsync(aggregate, ct),
            executionId,
            ct,
            taskId,
            employeeId);

        var resultResponse = new CodeReviewResultDto(
            repository,
            commitSha,
            aggregate.OverallScore,
            aggregate.Risk,
            aggregate.TechnicalDebt,
            aggregate.Scores,
            aggregate.PositiveFindings,
            aggregate.Issues,
            aggregate.Recommendations,
            aggregate.NextActions);

        var pullRequestText = pullRequestNumber is null ? string.Empty : pullRequestNumber.Value.ToString();
        var summaryText = string.IsNullOrWhiteSpace(summaries.ExecutiveSummary)
            ? summaries.DeveloperSummary
            : summaries.ExecutiveSummary;

        await _repository.AddSuccessLogAsync(
            CodeReviewRunLogEntry.CreateSuccess(
                executionId,
                organization,
                repository,
                branch,
                commitSha,
                pullRequestText,
                taskId,
                employeeId,
                aggregate.OverallScore ?? 0,
                aggregate.Risk,
                summaryText,
                now),
            ct);

        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Completed code review for {Repository}@{CommitSha} with score {Score}.",
            repository,
            commitSha,
            aggregate.OverallScore);

        return new CodeReviewWorkflowExecutionResult(
            executionId,
            resultResponse,
            summaries.ExecutiveSummary,
            summaries.DeveloperSummary,
            true,
            truncated);
    }

    private async Task<T> ExecuteStageAsync<T>(
        string stageName,
        Func<Task<T>> action,
        string executionId,
        CancellationToken ct,
        Guid? taskId = null,
        Guid? employeeId = null)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            await _repository.AddFailureLogAsync(
                CodeReviewRunLogEntry.CreateFailure(executionId, WorkflowName, stageName, ex.Message, taskId, employeeId, DateTime.UtcNow),
                ct);

            await _repository.SaveChangesAsync(ct);
            _logger.LogError(ex, "Code review failed during stage {Stage}.", stageName);
            throw;
        }
    }

    private static IReadOnlyList<GitHubCommitFile> FilterReviewableFiles(IReadOnlyList<GitHubCommitFile> files)
    {
        var ignoreExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".webp", ".bmp", ".pdf", ".zip", ".exe", ".dll",
            ".woff", ".woff2", ".ttf", ".eot", ".mp4", ".mov", ".gz", ".tar"
        };

        var ignoreNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "package-lock.json", "yarn.lock", "pnpm-lock.yaml", "gemfile.lock", "composer.lock", "poetry.lock", "cargo.lock"
        };

        var ignoreDirs = new[]
        {
            "node_modules/", "dist/", "build/", "vendor/", "bin/", "obj/", ".next/", "out/", ".git/"
        };

        var ignorePatterns = new[]
        {
            ".min.js", ".min.css", ".g.cs", ".designer.cs", ".generated.", ".snap"
        };

        var kept = new List<GitHubCommitFile>();

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.Filename) || string.IsNullOrWhiteSpace(file.Patch))
            {
                continue;
            }

            var name = file.Filename.ToLowerInvariant();
            var skip = ignoreExt.Any(extension => name.EndsWith(extension, StringComparison.Ordinal));

            if (!skip)
            {
                var baseName = name.Split('/').LastOrDefault() ?? string.Empty;
                skip = ignoreNames.Contains(baseName);
            }

            if (!skip)
            {
                skip = ignoreDirs.Any(dir => name.Contains(dir, StringComparison.Ordinal));
            }

            if (!skip)
            {
                skip = ignorePatterns.Any(pattern => name.Contains(pattern, StringComparison.Ordinal));
            }

            if (!skip)
            {
                kept.Add(file);
            }
        }

        return kept;
    }

    private static string BuildAiContext(IReadOnlyList<GitHubCommitFile> files, out bool truncated)
    {
        var sorted = files
            .OrderByDescending(x => x.Additions + x.Deletions)
            .ToList();

        const int cap = 12_000;
        var sb = new System.Text.StringBuilder(cap);
        truncated = false;

        foreach (var file in sorted)
        {
            var header = $"FILE: {file.Filename} ({file.Status}, +{file.Additions}/-{file.Deletions})";
            var block = $"{header}{Environment.NewLine}{file.Patch}{Environment.NewLine}{Environment.NewLine}";

            if (sb.Length + block.Length > cap)
            {
                var remaining = cap - sb.Length;
                if (remaining > 200)
                {
                    sb.Append(block[..remaining]);
                    sb.AppendLine("...[truncated]");
                }

                truncated = true;
                break;
            }

            sb.Append(block);
        }

        return sb.ToString();
    }

    private static IReadOnlyList<CodeReviewReviewerConfig> BuildReviewerConfigs()
    {
        return
        [
            new CodeReviewReviewerConfig("architecture", "Architecture Reviewer", true, "Clean Architecture, Layering, CQRS, DDD, Vertical Slice, Dependency Injection, Repository Pattern"),
            new CodeReviewReviewerConfig("solid", "SOLID Reviewer", true, "SRP, OCP, LSP, ISP, DIP"),
            new CodeReviewReviewerConfig("performance", "Performance Reviewer", true, "Database queries, LINQ, memory allocations, loops, async, caching, algorithmic complexity"),
            new CodeReviewReviewerConfig("security", "Security Reviewer", true, "OWASP Top 10, authentication, authorization, SQL injection, XSS, secrets, input validation, output encoding, logging sensitive data"),
            new CodeReviewReviewerConfig("quality", "Code Quality Reviewer", true, "Naming, readability, maintainability, magic numbers, duplicate code, large methods, large classes, code smells"),
            new CodeReviewReviewerConfig("database", "Database Reviewer", true, "EF Core, transactions, AsNoTracking, indexes, SaveChanges, N+1 queries, query optimization"),
            new CodeReviewReviewerConfig("testing", "Testing Reviewer", true, "Unit tests, integration tests, coverage estimation, mockability"),
            new CodeReviewReviewerConfig("api", "API Reviewer", true, "REST, DTOs, validation, status codes, pagination, filtering, versioning"),
            new CodeReviewReviewerConfig("positive", "Positive Reviewer", false, "Good practices, reusable code, good architecture, optimizations. Never return only negative feedback.")
        ];
    }

    private static CodeReviewAggregateResult AggregateFindings(IReadOnlyList<CodeReviewReviewerResult> results)
    {
        var weights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["architecture"] = 0.20m,
            ["security"] = 0.20m,
            ["performance"] = 0.15m,
            ["quality"] = 0.15m,
            ["solid"] = 0.10m,
            ["testing"] = 0.10m,
            ["database"] = 0.05m,
            ["api"] = 0.05m
        };

        var scores = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
        var seenIssues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var issues = new List<CodeReviewIssueDto>();
        var recsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recommendations = new List<string>();
        var positiveFindings = new List<string>();

        foreach (var result in results)
        {
            if (result.Score.HasValue && weights.ContainsKey(result.ReviewerKey))
            {
                scores[result.ReviewerKey] = result.Score.Value;
            }

            foreach (var issue in result.Issues)
            {
                var key = $"{issue.Title}|{issue.File}";
                if (seenIssues.Add(key))
                {
                    issues.Add(issue);
                }
            }

            foreach (var recommendation in result.Recommendations)
            {
                if (recsSet.Add(recommendation))
                {
                    recommendations.Add(recommendation);
                }
            }

            positiveFindings.AddRange(result.PositiveFindings.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        var overallScore = ComputeOverallScore(scores, weights);
        var sevRank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["critical"] = 0,
            ["high"] = 1,
            ["medium"] = 2,
            ["low"] = 3
        };

        issues.Sort((a, b) => sevRank.GetValueOrDefault(a.Severity, 2).CompareTo(sevRank.GetValueOrDefault(b.Severity, 2)));

        var risk = "Unknown";
        var technicalDebt = "Unknown";

        if (overallScore is not null)
        {
            if (overallScore >= 85)
            {
                risk = "Low";
                technicalDebt = "Low";
            }
            else if (overallScore >= 70)
            {
                risk = "Medium";
                technicalDebt = "Medium";
            }
            else
            {
                risk = "High";
                technicalDebt = "High";
            }
        }

        return new CodeReviewAggregateResult(
            scores,
            overallScore,
            risk,
            technicalDebt,
            issues,
            recommendations,
            positiveFindings,
            recommendations.Take(5).ToArray());
    }

    private static int? ComputeOverallScore(Dictionary<string, int?> scores, Dictionary<string, decimal> weights)
    {
        decimal weightedSum = 0;
        decimal weightTotal = 0;

        foreach (var (key, weight) in weights)
        {
            if (scores.TryGetValue(key, out var score) && score.HasValue)
            {
                weightedSum += score.Value * weight;
                weightTotal += weight;
            }
        }

        if (weightTotal <= 0)
        {
            return null;
        }

        return (int)Math.Round(weightedSum / weightTotal, MidpointRounding.AwayFromZero);
    }

    private sealed record CodeReviewAggregateResult(
        IReadOnlyDictionary<string, int?> Scores,
        int? OverallScore,
        string Risk,
        string TechnicalDebt,
        IReadOnlyList<CodeReviewIssueDto> Issues,
        IReadOnlyList<string> Recommendations,
        IReadOnlyList<string> PositiveFindings,
        IReadOnlyList<string> NextActions);
}
