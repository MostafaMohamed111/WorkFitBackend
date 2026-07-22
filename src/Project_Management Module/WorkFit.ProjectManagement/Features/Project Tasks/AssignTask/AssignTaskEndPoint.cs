using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;

public sealed class AssignTaskEndPoint : Endpoint<AssignTaskRequest, Guid>
{
    private readonly IMediator _mediator;
    public AssignTaskEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/tasks/{id}/assign");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(AssignTaskRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new AssignTaskCommand(req.ProjectId, taskId, req.AssigneeId,req.AllocationPercentage), ct);
        await Send.OkAsync(result, ct);
    }
}