using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface IGitHubCodeReviewService
{
    Task<GitHubRepositoryMetadata> GetRepositoryMetadataAsync(string organization, string repository, string? accessToken, CancellationToken ct);
    Task<GitHubRepositoryCreationResult> CreateRepositoryAsync(string organization, string repository, string? accessToken, string? description, CancellationToken ct);
    Task<GitHubBranchMetadata> GetBranchAsync(string organization, string repository, string branchName, string? accessToken, CancellationToken ct);
    Task<GitHubBranchCreationResult> CreateBranchAsync(string organization, string repository, string branchName, string baseBranchName, string? accessToken, CancellationToken ct);
    Task<GitHubCommitSnapshot> GetCommitAsync(string organization, string repository, string commitSha, string? accessToken, CancellationToken ct);
    Task<GitHubPullRequestSnapshot> GetPullRequestAsync(string organization, string repository, int pullRequestNumber, string? accessToken, CancellationToken ct);
    Task<GitHubComparisonSnapshot> GetComparisonAsync(string organization, string repository, string baseRef, string headRef, string? accessToken, CancellationToken ct);
}
