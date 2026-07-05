using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

public sealed class GetProjectByIdEndpoint : Endpoint<GetProjectByIdRequest, ProjectDetailDto>
{
    private readonly IMediator _mediator;

    public GetProjectByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/projects/{id}");
        Roles("TeamLeader");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<ProjectDetailDto>(200)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(GetProjectByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(req.Id), ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
