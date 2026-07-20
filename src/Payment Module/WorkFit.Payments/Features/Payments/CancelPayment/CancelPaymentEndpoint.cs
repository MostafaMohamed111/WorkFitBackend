using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CancelPayment;

public sealed class CancelPaymentEndpoint : Endpoint<CancelPaymentRequest, PaymentDto>
{
    private readonly IMediator _mediator;

    public CancelPaymentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/payments/{paymentId}/cancel");
        Roles("SuperAdmin", "Admin", "OrganizationOwner");
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentDto>(200)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(CancelPaymentRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPaymentCommand(req.PaymentId), ct);
        await Send.OkAsync(result, ct);
    }
}
