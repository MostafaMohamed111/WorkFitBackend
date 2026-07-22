using FastEndpoints;

using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class ArchiveProjectEndpoint : EndpointWithoutRequest<Guid>
{
    private readonly IMediator _mediator;

    public ArchiveProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/projects/{id}/archive");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
        Description(b => b
            .Produces<Guid>(200)
            .Produces(404));
    }

    public override async Task HandleAsync( CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var result = await _mediator.Send(new ArchiveProjectCommand(projectId), ct);

        await Send.OkAsync(result, ct);
    }
}
