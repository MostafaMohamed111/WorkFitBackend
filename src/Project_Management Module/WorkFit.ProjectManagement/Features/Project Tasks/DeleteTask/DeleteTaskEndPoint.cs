using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.DeleteTask;

public sealed class DeleteTaskEndPoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    public DeleteTaskEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }


    public override void Configure()
    {
        Delete("/api/tasks/{id}");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        await _mediator.Send(new DeleteTaskCommand(taskId));
        await Send.OkAsync();
    }
}