using FastEndpoints;

using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class ArchiveProjectEndpoint : Endpoint<ArchiveProjectRequest, ProjectArchivedDto>
{
    private readonly IMediator _mediator;

    public ArchiveProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/projects/{id}");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<ProjectArchivedDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(ArchiveProjectRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveProjectCommand(req.Id), ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
