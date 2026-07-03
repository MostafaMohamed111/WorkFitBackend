using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.UpdateAssignment;

public sealed class UpdateAssignmentEndPoint : Endpoint<UpdateAssignmentRequest>
{
    private readonly IMediator _mediator;

    public UpdateAssignmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/projects/{id}/assignments/{asgId}");
        AllowAnonymous();
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(UpdateAssignmentRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var assignmentId = Route<Guid>("asgId");

        var command = new UpdateAssignmentCommand(
            projectId,
            assignmentId,
            req.AllocationPercentage,
            req.RoleOnProject,
            req.EndDate);

        await _mediator.Send(command, ct);
        await Send.NoContentAsync(ct);
    }
}
