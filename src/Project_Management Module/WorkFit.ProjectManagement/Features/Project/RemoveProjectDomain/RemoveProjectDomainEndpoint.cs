using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.RemoveProjectDomain;

public sealed class RemoveProjectDomainEndpoint : Endpoint<RemoveProjectDomainRequest, DomainDeletedDto>
{
    private readonly IMediator _mediator;

    public RemoveProjectDomainEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/projects/{id}/domains/{domainId}");
        Options(x => x.WithTags("Project Management"));
        Description(b => b
            .Produces<DomainDeletedDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(RemoveProjectDomainRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveProjectDomainCommand(req.Id, req.DomainId), ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
