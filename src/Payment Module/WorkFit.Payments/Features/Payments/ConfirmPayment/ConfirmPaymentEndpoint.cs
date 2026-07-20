using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.ConfirmPayment;

public sealed class ConfirmPaymentEndpoint : Endpoint<ConfirmPaymentRequest, PaymentDto>
{
    private readonly IMediator _mediator;

    public ConfirmPaymentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/payments/{paymentId}/confirm");
        Roles("SuperAdmin", "Admin", "OrganizationOwner");
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentDto>(200)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(ConfirmPaymentRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmPaymentCommand(req.PaymentId), ct);
        await Send.OkAsync(result, ct);
    }
}
