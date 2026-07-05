using FastEndpoints;

using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

public sealed class UpdateProjectEndpoint : Endpoint<UpdateProjectRequest, ProjectUpdatedDto>
{
    private readonly IMediator _mediator;

    public UpdateProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/projects/{id}");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<ProjectUpdatedDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        var command = new UpdateProjectCommand(
            req.Id,
            req.Name,
            req.Description,
            req.Status,
            req.EndDate,
            req.RequiredSkills);

        var result = await _mediator.Send(command, ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
