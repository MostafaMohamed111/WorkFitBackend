using WorkFit.CodeReview.Features.GitHubCodeReview;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface ICodeReviewWorkflowService
{
    Task<CodeReviewWorkflowExecutionResult> ExecuteAsync(
        ReviewGitHubCommitCommand request,
        CancellationToken ct);

    Task<CodeReviewWorkflowExecutionResult> ExecuteTaskAsync(
        ReviewTaskGitHubChangesCommand request,
        CancellationToken ct);
}
