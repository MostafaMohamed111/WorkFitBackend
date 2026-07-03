using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.RemoveAssignment;

public sealed class RemoveAssignmentEndPoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public RemoveAssignmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/projects/{id}/assignments/{asgId}");
        AllowAnonymous();
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var assignmentId = Route<Guid>("asgId");

        var command = new RemoveAssignmentCommand(projectId, assignmentId);
        await _mediator.Send(command, ct);
        await Send.NoContentAsync(ct);
    }
}
