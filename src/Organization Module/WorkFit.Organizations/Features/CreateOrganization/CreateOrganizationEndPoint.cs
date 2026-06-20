
using FastEndpoints;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.CreateOrganization;

public sealed class CreateOrganizationEndPoint : Endpoint<CreateOrganizationRequest, Guid>
{
    private readonly IMediator _mediator;

    public CreateOrganizationEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/organizations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateOrganizationRequest req, CancellationToken ct)
    {
        var command = new CreateOrganizationCommand(req.Name, req.UserId);
        var organizationId = await _mediator.Send(command, ct);
        await Send.OkAsync(organizationId, ct);
    }
}
