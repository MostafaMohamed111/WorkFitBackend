
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.RegisterWithRoleForTesting;

public sealed class RegisterWithRoleForTestingEndPoint : Endpoint<RegisterWithRoleForTestingRequest>
{
    private readonly IMediator _mediator;

    public RegisterWithRoleForTestingEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Post("/api/identity/register");
        Options(x => x.WithTags("Identity"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterWithRoleForTestingRequest req, CancellationToken ct)
    {
        var command = new RegisterWithRoleForTestingCommand(req.Email, req.Password, req.Roles,req.Name);
        await _mediator.Send(command, ct);
    }
}
