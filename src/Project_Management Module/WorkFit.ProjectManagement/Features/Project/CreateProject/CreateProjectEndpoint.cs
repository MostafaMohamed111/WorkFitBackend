using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed class CreateProjectEndpoint : Endpoint<CreateProjectRequest, Guid>
{
    private readonly IMediator _mediator;

    public CreateProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/projects");
        Roles("TeamLeader");
        Options(x => x.WithTags("Project Management"));
        Description(static b => b
            .Produces<Guid>(200)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403));
    }

    public override async Task HandleAsync(CreateProjectRequest req, CancellationToken ct)
    {
        var command = new CreateProjectCommand(
            req.Name,
            req.AttatchedDocumentIds,
            req.Description,
            req.DepartmentId,
            req.StartDate,
            req.EndDate,
            req.Status,
            req.RequiredSkills);

        var projectId = await _mediator.Send(command, ct);

        await Send.OkAsync(projectId);
    }
}
