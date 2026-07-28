namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public sealed record CodeReviewIssueDto(
    string Title,
    string Severity,
    string Detail,
    string Recommendation,
    string File,
    string Reviewer);
