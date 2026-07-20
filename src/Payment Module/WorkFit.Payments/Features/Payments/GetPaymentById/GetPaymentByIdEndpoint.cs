using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.GetPaymentById;

public sealed class GetPaymentByIdEndpoint : Endpoint<GetPaymentByIdRequest, PaymentDto>
{
    private readonly IMediator _mediator;

    public GetPaymentByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/payments/{paymentId}");
        Roles("SuperAdmin", "Admin", "OrganizationOwner", "Employee");
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentDto>(200)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403)
            .Produces(404));
    }

    public override async Task HandleAsync(GetPaymentByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(req.PaymentId), ct);
        await Send.OkAsync(result, ct);
    }
}
