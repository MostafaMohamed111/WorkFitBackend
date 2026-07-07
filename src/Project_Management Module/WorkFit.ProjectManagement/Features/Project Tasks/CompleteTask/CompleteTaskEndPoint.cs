using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;
public sealed class CompleteTaskEndPoint : EndpointWithoutRequest<CompleteTaskResponse>
{
    private readonly IMediator _mediator;
    public CompleteTaskEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}/complete");
        Options(x => x.WithTags("Project Management"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new CompleteTaskCommand(taskId), ct);
        await Send.OkAsync(result, ct);
    }
}