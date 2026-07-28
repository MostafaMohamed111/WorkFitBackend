using Microsoft.Extensions.Logging;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class CodeReviewReviewerService : ICodeReviewReviewerService
{
    private readonly ICodeReviewAgentService _agentService;
    private readonly ILogger<CodeReviewReviewerService> _logger;

    public CodeReviewReviewerService(
        ICodeReviewAgentService agentService,
        ILogger<CodeReviewReviewerService> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CodeReviewReviewerResult>> RunReviewersAsync(
        IReadOnlyList<CodeReviewReviewerConfig> reviewers,
        string repository,
        string commitSha,
        string codeContext,
        CancellationToken ct)
    {
        var tasks = reviewers.Select(async reviewer =>
        {
            try
            {
                return await _agentService.ReviewAsync(reviewer, repository, commitSha, codeContext, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reviewer {ReviewerKey} failed. Returning normalized fallback.", reviewer.ReviewerKey);
                return new CodeReviewReviewerResult(
                    reviewer.ReviewerKey,
                    reviewer.ReviewerName,
                    repository,
                    commitSha,
                    null,
                    Array.Empty<CodeReviewIssueDto>(),
                    [$"AI review could not be completed: {ex.Message}"],
                    Array.Empty<string>());
            }
        });

        return await Task.WhenAll(tasks);
    }
}
