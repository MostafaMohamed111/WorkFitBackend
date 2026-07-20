using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CreatePayment;

public sealed class CreatePaymentEndpoint : Endpoint<CreatePaymentRequest, PaymentDto>
{
    private readonly IMediator _mediator;

    public CreatePaymentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/payments");
        Roles("SuperAdmin", "Admin", "OrganizationOwner");
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentDto>(201)
            .ProducesProblem(400)
            .Produces(401)
            .Produces(403));
    }

    public override async Task HandleAsync(CreatePaymentRequest req, CancellationToken ct)
    {
        var command = new CreatePaymentCommand(
            req.ReferenceId,
            req.ReferenceType,
            req.Amount,
            req.Currency,
            req.Description,
            req.MockOutcome);

        var result = await _mediator.Send(command, ct);

        await Send.CreatedAtAsync<GetPaymentById.GetPaymentByIdEndpoint>(
            new { id = result.Id },
            result,
            cancellation: ct);
    }
}
