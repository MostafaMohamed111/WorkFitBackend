using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskAllocation;
public sealed class SetTaskAllocationEndPoint : Endpoint<SetTaskAllocationRequest, Guid>
{
    private readonly IMediator _mediator;
    public SetTaskAllocationEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}/allocation");
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(SetTaskAllocationRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new SetTaskAllocationCommand(taskId, req.AllocationPercentage), ct);
        await Send.OkAsync(result, ct);
    }
}