using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed record ReviewGitHubCommitCommand(
    string Organization,
    string Repository,
    string Branch,
    string CommitSha,
    int? PullRequestNumber,
    string? AccessToken) : IRequest<CodeReviewWorkflowExecutionResult>;
