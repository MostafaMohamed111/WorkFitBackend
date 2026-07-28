namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public sealed record CodeReviewResultDto(
    string Repository,
    string Commit,
    int? OverallScore,
    string Risk,
    string TechnicalDebt,
    IReadOnlyDictionary<string, int?> Scores,
    IReadOnlyList<string> PositiveFindings,
    IReadOnlyList<CodeReviewIssueDto> Issues,
    IReadOnlyList<string> Recommendations,
    IReadOnlyList<string> NextActions);
