using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.ForgotPassword;

public sealed class ForgotPasswordEndPoint : Endpoint<ForgotPasswordRequest>
{
    private readonly IMediator _mediator;

    public ForgotPasswordEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/forgot-password");
        Options(x => x.WithTags("Identity"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await _mediator.Send(command, ct);

        await Send.NoContentAsync(ct);
    }
}