using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.GetPaymentStatus;

public sealed class GetPaymentStatusEndpoint : Endpoint<GetPaymentStatusRequest, PaymentStatusDto>
{
    private readonly IMediator _mediator;

    public GetPaymentStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/payments/{paymentId}/status");
        Roles("SuperAdmin", "Admin", "OrganizationOwner", "Employee");
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentStatusDto>(200)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(GetPaymentStatusRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentStatusQuery(req.PaymentId), ct);
        await Send.OkAsync(result, ct);
    }
}
