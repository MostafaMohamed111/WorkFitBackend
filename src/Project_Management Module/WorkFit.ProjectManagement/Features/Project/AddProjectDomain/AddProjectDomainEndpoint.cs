using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.AddProjectDomain;

public sealed class AddProjectDomainEndpoint : Endpoint<AddProjectDomainRequest, ProjectDomainTaggedDto>
{
    private readonly IMediator _mediator;

    public AddProjectDomainEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/projects/{id}/domains");
        Roles("TeamLeader");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<ProjectDomainTaggedDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(AddProjectDomainRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddProjectDomainCommand(req.Id, req.DomainId), ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
