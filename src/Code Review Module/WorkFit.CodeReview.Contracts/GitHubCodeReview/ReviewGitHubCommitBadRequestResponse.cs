namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public sealed record ReviewGitHubCommitBadRequestResponse(
    string Error,
    IReadOnlyList<string> MissingFields);
