using WorkFit.CodeReview.Features.GitHubCodeReview;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface ICodeReviewWorkflowService
{
    Task<CodeReviewWorkflowExecutionResult> ExecuteAsync(
        ReviewGitHubCommitCommand request,
        CancellationToken ct);
}
