using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.CodeReview.Features.GitHubCodeReview;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;

public sealed class TakeCompleteWithCodeReviewEndPoint : EndpointWithoutRequest<CodeReviewWorkflowExecutionResult>
{
    private readonly IMediator _mediator;

    public TakeCompleteWithCodeReviewEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}/take-complete-with-code-review");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new TakeCompleteWithCodeReviewCommand(taskId), ct);
        await Send.OkAsync(result, ct);
    }
}
