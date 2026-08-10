using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface IGitHubCodeReviewService
{
    Task<GitHubRepositoryMetadata> GetRepositoryMetadataAsync(string organization, string repository, string? accessToken, CancellationToken ct);
    Task<GitHubCommitSnapshot> GetCommitAsync(string organization, string repository, string commitSha, string? accessToken, CancellationToken ct);
    Task<GitHubPullRequestSnapshot> GetPullRequestAsync(string organization, string repository, int pullRequestNumber, string? accessToken, CancellationToken ct);
    Task<GitHubComparisonSnapshot> GetComparisonAsync(string organization, string repository, string baseRef, string headRef, string? accessToken, CancellationToken ct);
}
