using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class DeleteProjectEndpoint : EndpointWithoutRequest<Guid>
{
    private readonly IMediator _mediator;

    public DeleteProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/projects/{id}");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin");
        Description(b => b
            .Produces<Guid>(200)
            .Produces(404)
            .Produces(409));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var result = await _mediator.Send(new ArchiveProjectCommand(projectId), ct);

        await Send.OkAsync(result, ct);
    }
}
