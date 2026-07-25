using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Features.Payments;
using WorkFit.Payments.Infrastructure.Data;

namespace WorkFit.Payments.Features.Payments.CancelCheckoutSession;

public sealed class CancelCheckoutSessionEndpoint : Endpoint<CancelCheckoutSessionRequest, PaymentDto>
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public CancelCheckoutSessionEndpoint(
        PaymentDbContext context,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _databaseMigrator = databaseMigrator;
    }

    public override void Configure()
    {
        Post("/api/payments/checkout-session/cancel");
        AllowAnonymous();
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<PaymentDto>(200)
            .ProducesProblem(400)
            .Produces(404));
    }

    public override async Task HandleAsync(CancelCheckoutSessionRequest req, CancellationToken ct)
    {
        await _databaseMigrator.EnsureMigratedAsync(ct);

        var payment = await _context.Payments
            .SingleOrDefaultAsync(
                x => x.ReferenceId == req.ReferenceId && x.ReferenceType == req.ReferenceType,
                ct)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.Status != PaymentStatus.Succeeded)
        {
            payment.MarkStatus(PaymentStatus.Cancelled);
            await _context.SaveChangesAsync(ct);
        }

        await Send.OkAsync(payment.ToDto(), ct);
    }
}
