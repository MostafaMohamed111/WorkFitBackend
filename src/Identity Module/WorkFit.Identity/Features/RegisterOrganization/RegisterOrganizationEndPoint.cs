
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterOrganization;

public sealed class RegisterOrganizationEndPoint :Endpoint<RegisterOrganizationRequest>
{
    private readonly IMediator _mediator;

    public RegisterOrganizationEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/register-organization");
        Options(x => x.WithTags("Identity"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterOrganizationRequest req, CancellationToken ct)
    {
        var command = new RegisterOrganizationCommand(req.Email,
            req.Password,
            req.ConfirmPassword,
            req.OrganizationName);
        await _mediator.Send(command, ct);
        await Send.NoContentAsync(ct);
    }
}
