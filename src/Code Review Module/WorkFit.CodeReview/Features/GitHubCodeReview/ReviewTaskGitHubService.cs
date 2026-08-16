using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

internal sealed class ReviewTaskGitHubService : IReviewTaskGitHub
{
    private readonly IMediator _mediator;

    public ReviewTaskGitHubService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<CodeReviewWorkflowExecutionResult> ReviewTaskAsync(
        Guid taskId,
        Guid? employeeId,
        string organization,
        string repository,
        string? branch,
        int? pullRequestNumber,
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(
            new ReviewTaskGitHubChangesCommand(
                taskId,
                employeeId,
                organization,
                repository,
                branch,
                pullRequestNumber,
                accessToken),
            cancellationToken);
    }
}