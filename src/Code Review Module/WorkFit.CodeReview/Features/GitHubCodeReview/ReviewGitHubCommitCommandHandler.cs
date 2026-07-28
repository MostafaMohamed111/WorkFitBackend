using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed class ReviewGitHubCommitCommandHandler : IRequestHandler<ReviewGitHubCommitCommand, CodeReviewWorkflowExecutionResult>
{
    private readonly ICodeReviewWorkflowService _workflowService;

    public ReviewGitHubCommitCommandHandler(ICodeReviewWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public Task<CodeReviewWorkflowExecutionResult> Handle(ReviewGitHubCommitCommand request, CancellationToken cancellationToken = default)
    {
        return _workflowService.ExecuteAsync(request, cancellationToken);
    }
}
