using FastEndpoints;

using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectDomains;

public sealed class GetProjectDomainsEndpoint : Endpoint<GetProjectDomainsRequest, ProjectDomainListDto>
{
    private readonly IMediator _mediator;

    public GetProjectDomainsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/projects/{id}/domains");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<ProjectDomainListDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(GetProjectDomainsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectDomainsQuery(req.Id), ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
