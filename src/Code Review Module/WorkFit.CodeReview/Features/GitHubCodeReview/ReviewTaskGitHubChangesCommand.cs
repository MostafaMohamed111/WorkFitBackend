using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed record ReviewTaskGitHubChangesCommand(
    Guid TaskId,
    Guid? EmployeeId,
    string Organization,
    string Repository,
    string? Branch,
    int? PullRequestNumber,
    string? AccessToken) : IRequest<CodeReviewWorkflowExecutionResult>;
