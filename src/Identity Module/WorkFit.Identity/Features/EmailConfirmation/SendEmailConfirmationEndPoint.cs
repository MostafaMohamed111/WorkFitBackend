using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.EmailConfirmation;

public sealed class SendEmailConfirmationEndPoint : Endpoint<SendEmailConfirmationRequest>
{
    private readonly IMediator _mediator;

    public SendEmailConfirmationEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/email-confirmation");
        Options(x => x.WithTags("Identity"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(SendEmailConfirmationRequest request, CancellationToken ct)
    {
        var command = new SendEmailConfirmationCommand(request.Email);
        await _mediator.Send(command, ct);

        await Send.NoContentAsync(ct);
    }
}