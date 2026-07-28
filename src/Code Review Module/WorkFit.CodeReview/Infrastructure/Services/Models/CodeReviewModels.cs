using WorkFit.CodeReview.Contracts.GitHubCodeReview;

namespace WorkFit.CodeReview.Infrastructure.Services.Models;

public sealed record GitHubRepositoryMetadata(string DefaultBranch, string RawJson);

public sealed record GitHubCommitFile(string Filename, string Status, int Additions, int Deletions, string Patch);

public sealed record GitHubCommitSnapshot(string Sha, string AuthorName, string Message, IReadOnlyList<GitHubCommitFile> Files);

public sealed record CodeReviewReviewerConfig(
    string ReviewerKey,
    string ReviewerName,
    bool Scored,
    string Focus);

public sealed record CodeReviewReviewerResult(
    string ReviewerKey,
    string ReviewerName,
    string Repository,
    string CommitSha,
    int? Score,
    IReadOnlyList<CodeReviewIssueDto> Issues,
    IReadOnlyList<string> Recommendations,
    IReadOnlyList<string> PositiveFindings);

public sealed record CodeReviewSummaryResult(string ExecutiveSummary, string DeveloperSummary);
