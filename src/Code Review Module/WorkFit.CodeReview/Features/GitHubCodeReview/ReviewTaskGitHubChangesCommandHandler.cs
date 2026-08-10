using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed class ReviewTaskGitHubChangesCommandHandler : IRequestHandler<ReviewTaskGitHubChangesCommand, CodeReviewWorkflowExecutionResult>
{
    private readonly ICodeReviewWorkflowService _workflowService;

    public ReviewTaskGitHubChangesCommandHandler(ICodeReviewWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public Task<CodeReviewWorkflowExecutionResult> Handle(ReviewTaskGitHubChangesCommand request, CancellationToken cancellationToken = default)
    {
        return _workflowService.ExecuteTaskAsync(request, cancellationToken);
    }
}
