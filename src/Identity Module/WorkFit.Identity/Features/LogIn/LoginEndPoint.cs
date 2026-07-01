

using FastEndpoints;
using Microsoft.AspNetCore.Identity.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.LogIn;

public sealed class LoginEndPoint : Endpoint<LoginRequest, string>
{
    private readonly IMediator _mediator;

    public LoginEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var command = new LoginCommand(req.Email, req.Password);
        var result = await _mediator.Send(command, ct);

        await Send.OkAsync(result, cancellation: ct);

    }
}
