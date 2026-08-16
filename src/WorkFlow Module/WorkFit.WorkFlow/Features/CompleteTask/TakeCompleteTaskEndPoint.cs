using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.CompleteTask;

public sealed class TakeCompleteTaskEndPoint : EndpointWithoutRequest<TakeCompleteTaskResponse>
{
    private readonly IMediator _mediator;

    public TakeCompleteTaskEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}/take-complete-with-code-review");
        Options(x => x.WithTags("WorkFlow", "Project Management"));
        Roles("TeamLeader", "TeamLead", "Employee", "OrganizationOwner", "Admin", "SuperAdmin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new TakeCompleteTaskCommand(taskId), ct);
        await Send.OkAsync(result, ct);
    }
}