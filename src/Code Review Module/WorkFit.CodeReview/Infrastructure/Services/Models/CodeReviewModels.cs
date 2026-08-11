using WorkFit.CodeReview.Contracts.GitHubCodeReview;

namespace WorkFit.CodeReview.Infrastructure.Services.Models;

public sealed record GitHubRepositoryMetadata(long Id, string Name, string DefaultBranch, string RawJson);

public sealed record GitHubRepositoryCreationResult(long Id, string Name, string DefaultBranch, string RawJson);

public sealed record GitHubBranchCreationResult(string Name, string Sha, string NodeId, string RawJson);

public sealed record GitHubBranchMetadata(string Name, string Sha, string NodeId, string RawJson);

public sealed record GitHubCommitFile(string Filename, string Status, int Additions, int Deletions, string Patch);

public sealed record GitHubCommitSnapshot(string Sha, string AuthorName, string Message, IReadOnlyList<GitHubCommitFile> Files);

public sealed record GitHubPullRequestSnapshot(string BaseBranch, string HeadBranch, string HeadSha, string RawJson);

public sealed record GitHubComparisonSnapshot(string HeadSha, IReadOnlyList<GitHubCommitFile> Files, string RawJson);

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
