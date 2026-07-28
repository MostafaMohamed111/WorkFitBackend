namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public sealed record ReviewGitHubCommitRequest(
    string organization,
    string repository,
    string branch,
    string commitSha,
    int? pullRequestNumber,
    string? accessToken);
