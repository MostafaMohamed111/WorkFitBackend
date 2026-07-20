using Microsoft.EntityFrameworkCore;
using Stripe;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Domain.Entities;
using WorkFit.Payments.Infrastructure.Data;

namespace WorkFit.Payments.Infrastructure.Services;

public sealed class PaymentWebhookService : IPaymentWebhookService
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public PaymentWebhookService(
        PaymentDbContext context,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _databaseMigrator = databaseMigrator;
    }

    public async Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        await _databaseMigrator.EnsureMigratedAsync(cancellationToken);

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return;
        }

        var payment = await _context.Payments
            .SingleOrDefaultAsync(x => x.ProviderPaymentId == paymentIntent.Id, cancellationToken);

        if (payment is null)
        {
            payment = Payment.Create(
                GetMetadata(paymentIntent, "reference_id", paymentIntent.Id),
                GetMetadata(paymentIntent, "reference_type", "Unknown"),
                paymentIntent.Amount / 100m,
                paymentIntent.Currency ?? "usd",
                PaymentProvider.Stripe,
                paymentIntent.Id,
                GetLatestChargeId(paymentIntent),
                MapStatus(paymentIntent.Status),
                paymentIntent.ClientSecret);

            _context.Add(payment);
        }
        else
        {
            payment.UpdateGatewayState(
                paymentIntent.Id,
                GetLatestChargeId(paymentIntent),
                MapStatus(paymentIntent.Status),
                paymentIntent.ClientSecret);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string GetMetadata(PaymentIntent paymentIntent, string key, string fallback)
    {
        return paymentIntent.Metadata != null &&
               paymentIntent.Metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static PaymentStatus MapStatus(string? stripeStatus)
    {
        return stripeStatus?.ToLowerInvariant() switch
        {
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Cancelled,
            "requires_payment_method" => PaymentStatus.Failed,
            "requires_action" => PaymentStatus.Pending,
            "processing" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            _ => PaymentStatus.Pending
        };
    }

    private static string? GetLatestChargeId(PaymentIntent paymentIntent)
    {
        var latestChargeIdProperty = paymentIntent.GetType().GetProperty("LatestChargeId");
        var latestChargeProperty = paymentIntent.GetType().GetProperty("LatestCharge");

        return latestChargeIdProperty?.GetValue(paymentIntent)?.ToString()
            ?? latestChargeProperty?.GetValue(paymentIntent)?.ToString();
    }
}
