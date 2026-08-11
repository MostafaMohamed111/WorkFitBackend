using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.CodeReview.Infrastructure.Options;
using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class GitHubCodeReviewService : IGitHubCodeReviewService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<CodeReviewOptions> _options;
    private readonly ILogger<GitHubCodeReviewService> _logger;

    public GitHubCodeReviewService(
        IHttpClientFactory httpClientFactory,
        IOptions<CodeReviewOptions> options,
        ILogger<GitHubCodeReviewService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<GitHubRepositoryMetadata> GetRepositoryMetadataAsync(string organization, string repository, string? accessToken, CancellationToken ct)
    {
        var relativeUrl = $"repos/{organization}/{repository}";
        var json = await SendAndReadJsonAsync(relativeUrl, accessToken, ct);
        using var document = JsonDocument.Parse(json);
        return ReadRepositoryMetadata(document.RootElement, json);
    }

    public async Task<GitHubRepositoryCreationResult> CreateRepositoryAsync(string organization, string repository, string? accessToken, string? description, CancellationToken ct)
    {
        var options = _options.Value.GitHub;
        var client = _httpClientFactory.CreateClient("CodeReviewGitHub");
        client.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"orgs/{organization}/repos");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", options.ApiVersion);
        request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);
        ApplyAuthorizationHeader(request, accessToken, options.PersonalAccessToken);
        request.Content = CreateJsonContent(new
        {
            name = repository,
            description,
            auto_init = true,
            @private = true
        });

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(json);
            return ReadRepositoryCreationResult(document.RootElement, json);
        }

        if (response.StatusCode is HttpStatusCode.UnprocessableEntity or HttpStatusCode.Conflict)
        {
            var existing = await GetRepositoryMetadataAsync(organization, repository, accessToken, ct);
            return new GitHubRepositoryCreationResult(existing.Id, existing.Name, existing.DefaultBranch, existing.RawJson);
        }

        await EnsureSuccessAsync(response, "GitHub", $"orgs/{organization}/repos");
        throw new InvalidOperationException("GitHub repository creation failed.");
    }

    public async Task<GitHubBranchMetadata> GetBranchAsync(string organization, string repository, string branchName, string? accessToken, CancellationToken ct)
    {
        var relativeUrl = $"repos/{organization}/{repository}/branches/{branchName}";
        var json = await SendAndReadJsonAsync(relativeUrl, accessToken, ct);
        using var document = JsonDocument.Parse(json);
        return ReadBranchMetadata(document.RootElement, json);
    }

    public async Task<GitHubBranchCreationResult> CreateBranchAsync(string organization, string repository, string branchName, string baseBranchName, string? accessToken, CancellationToken ct)
    {
        var baseBranch = await GetBranchAsync(organization, repository, baseBranchName, accessToken, ct);

        var options = _options.Value.GitHub;
        var client = _httpClientFactory.CreateClient("CodeReviewGitHub");
        client.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"repos/{organization}/{repository}/git/refs");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", options.ApiVersion);
        request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);
        ApplyAuthorizationHeader(request, accessToken, options.PersonalAccessToken);
        request.Content = CreateJsonContent(new
        {
            @ref = $"refs/heads/{branchName}",
            sha = baseBranch.Sha
        });

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(json);
            return ReadBranchCreationResult(document.RootElement, json);
        }

        if (response.StatusCode is HttpStatusCode.UnprocessableEntity or HttpStatusCode.Conflict)
        {
            var existing = await GetBranchAsync(organization, repository, branchName, accessToken, ct);
            return new GitHubBranchCreationResult(existing.Name, existing.Sha, existing.NodeId, existing.RawJson);
        }

        await EnsureSuccessAsync(response, "GitHub", $"repos/{organization}/{repository}/git/refs");
        throw new InvalidOperationException("GitHub branch creation failed.");
    }

    public async Task<GitHubCommitSnapshot> GetCommitAsync(string organization, string repository, string commitSha, string? accessToken, CancellationToken ct)
    {
        var relativeUrl = $"repos/{organization}/{repository}/commits/{commitSha}";
        var json = await SendAndReadJsonAsync(relativeUrl, accessToken, ct);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var sha = root.TryGetProperty("sha", out var shaElement)
            ? shaElement.GetString() ?? commitSha
            : commitSha;

        var authorName = string.Empty;
        if (root.TryGetProperty("commit", out var commitElement) &&
            commitElement.TryGetProperty("author", out var authorElement) &&
            authorElement.TryGetProperty("name", out var authorNameElement))
        {
            authorName = authorNameElement.GetString() ?? string.Empty;
        }

        var message = string.Empty;
        if (root.TryGetProperty("commit", out var commitElement2) &&
            commitElement2.TryGetProperty("message", out var messageElement))
        {
            message = messageElement.GetString() ?? string.Empty;
        }

        return new GitHubCommitSnapshot(sha, authorName, message, ParseFiles(root));
    }

    public async Task<GitHubPullRequestSnapshot> GetPullRequestAsync(string organization, string repository, int pullRequestNumber, string? accessToken, CancellationToken ct)
    {
        var relativeUrl = $"repos/{organization}/{repository}/pulls/{pullRequestNumber}";
        var json = await SendAndReadJsonAsync(relativeUrl, accessToken, ct);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var baseBranch = GetStringProperty(root, "base", "ref");
        var headBranch = GetStringProperty(root, "head", "ref");
        var headSha = GetStringProperty(root, "head", "sha");

        return new GitHubPullRequestSnapshot(baseBranch, headBranch, headSha, json);
    }

    public async Task<GitHubComparisonSnapshot> GetComparisonAsync(string organization, string repository, string baseRef, string headRef, string? accessToken, CancellationToken ct)
    {
        var relativeUrl = $"repos/{organization}/{repository}/compare/{baseRef}...{headRef}";
        var json = await SendAndReadJsonAsync(relativeUrl, accessToken, ct);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var headSha = string.Empty;
        if (root.TryGetProperty("commits", out var commitsElement) && commitsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var commitElement in commitsElement.EnumerateArray())
            {
                if (commitElement.TryGetProperty("sha", out var shaElement))
                {
                    headSha = shaElement.GetString() ?? string.Empty;
                }
            }
        }

        return new GitHubComparisonSnapshot(headSha, ParseFiles(root), json);
    }

    private async Task<string> SendAndReadJsonAsync(string relativeUrl, string? accessToken, CancellationToken ct)
    {
        var options = _options.Value.GitHub;
        var token = accessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = options.PersonalAccessToken;
        }

        var client = _httpClientFactory.CreateClient("CodeReviewGitHub");
        client.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);

        return await RetryAsync(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", options.ApiVersion);
            request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                throw new InvalidOperationException("GitHub access token is missing. Provide accessToken in the request body.");
            }

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            await EnsureSuccessAsync(response, "GitHub", relativeUrl);

            return await response.Content.ReadAsStringAsync(ct);
        }, ct);
    }

    private static StringContent CreateJsonContent<T>(T payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    private static void ApplyAuthorizationHeader(HttpRequestMessage request, string? accessToken, string? personalAccessToken)
    {
        var token = accessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = personalAccessToken;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("GitHub access token is missing. Provide accessToken in the request body.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static GitHubRepositoryMetadata ReadRepositoryMetadata(JsonElement root, string rawJson)
    {
        var id = root.TryGetProperty("id", out var idElement) && idElement.TryGetInt64(out var idValue)
            ? idValue
            : 0;

        var name = GetStringProperty(root, "name");
        if (string.IsNullOrWhiteSpace(name))
        {
            name = GetStringProperty(root, "full_name");
        }

        var defaultBranch = GetStringProperty(root, "default_branch");
        return new GitHubRepositoryMetadata(id, name, defaultBranch, rawJson);
    }

    private static GitHubRepositoryCreationResult ReadRepositoryCreationResult(JsonElement root, string rawJson)
    {
        var metadata = ReadRepositoryMetadata(root, rawJson);
        return new GitHubRepositoryCreationResult(metadata.Id, metadata.Name, metadata.DefaultBranch, metadata.RawJson);
    }

    private static GitHubBranchMetadata ReadBranchMetadata(JsonElement root, string rawJson)
    {
        var name = GetStringProperty(root, "name");
        if (string.IsNullOrWhiteSpace(name))
        {
            name = GetBranchNameFromRef(GetStringProperty(root, "ref"));
        }
        var nodeId = GetStringProperty(root, "node_id");

        var sha = string.Empty;
        if (root.TryGetProperty("commit", out var commitElement) &&
            commitElement.TryGetProperty("sha", out var shaElement))
        {
            sha = shaElement.GetString() ?? string.Empty;
        }

        return new GitHubBranchMetadata(name, sha, nodeId, rawJson);
    }

    private static GitHubBranchCreationResult ReadBranchCreationResult(JsonElement root, string rawJson)
    {
        var branch = ReadBranchMetadata(root, rawJson);
        return new GitHubBranchCreationResult(branch.Name, branch.Sha, branch.NodeId, branch.RawJson);
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < 3 && IsTransient(ex))
            {
                lastException = ex;
                var delay = TimeSpan.FromSeconds(attempt * 2);
                _logger.LogWarning(ex, "Transient GitHub failure on attempt {Attempt}. Retrying in {Delay}.", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }

        throw lastException ?? new InvalidOperationException("GitHub request failed.");
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException or TaskCanceledException;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string systemName, string relativeUrl)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"{systemName} request to '{relativeUrl}' failed with status {(int)response.StatusCode} ({response.StatusCode}). {body}");
    }

    private static IReadOnlyList<GitHubCommitFile> ParseFiles(JsonElement root)
    {
        var files = new List<GitHubCommitFile>();

        if (!root.TryGetProperty("files", out var filesElement) || filesElement.ValueKind != JsonValueKind.Array)
        {
            return files;
        }

        foreach (var fileElement in filesElement.EnumerateArray())
        {
            var filename = fileElement.TryGetProperty("filename", out var filenameElement)
                ? filenameElement.GetString() ?? string.Empty
                : string.Empty;

            var status = fileElement.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString() ?? string.Empty
                : string.Empty;

            var additions = fileElement.TryGetProperty("additions", out var additionsElement) && additionsElement.TryGetInt32(out var additionsValue)
                ? additionsValue
                : 0;

            var deletions = fileElement.TryGetProperty("deletions", out var deletionsElement) && deletionsElement.TryGetInt32(out var deletionsValue)
                ? deletionsValue
                : 0;

            var patch = fileElement.TryGetProperty("patch", out var patchElement)
                ? patchElement.GetString() ?? string.Empty
                : string.Empty;

            files.Add(new GitHubCommitFile(filename, status, additions, deletions, patch));
        }

        return files;
    }

    private static string GetStringProperty(JsonElement root, string parentProperty, string childProperty)
    {
        if (root.TryGetProperty(parentProperty, out var parentElement) &&
            parentElement.TryGetProperty(childProperty, out var childElement))
        {
            return childElement.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string GetStringProperty(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var element)
            ? element.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string GetBranchNameFromRef(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return string.Empty;
        }

        const string headsPrefix = "refs/heads/";
        if (reference.StartsWith(headsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return reference[headsPrefix.Length..];
        }

        return reference;
    }
}
