using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
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

        if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
        {
            await HandlePaymentIntentAsync(paymentIntent, cancellationToken);
            return;
        }

        if (stripeEvent.Data.Object is Session checkoutSession)
        {
            await HandleCheckoutSessionAsync(stripeEvent.Type, checkoutSession, cancellationToken);
            return;
        }
    }

    private async Task HandlePaymentIntentAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .SingleOrDefaultAsync(x => x.ProviderPaymentId == paymentIntent.Id, cancellationToken);

        if (payment is null)
        {
            payment = await FindByMetadataAsync(paymentIntent.Metadata, cancellationToken);
        }

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

    private async Task HandleCheckoutSessionAsync(string eventType, Session checkoutSession, CancellationToken cancellationToken)
    {
        var referenceId = GetMetadata(checkoutSession.Metadata, "reference_id", checkoutSession.Id);
        var referenceType = GetMetadata(checkoutSession.Metadata, "reference_type", "Unknown");
        var payment = await _context.Payments
            .SingleOrDefaultAsync(
                x => x.ProviderPaymentId == checkoutSession.Id
                    || (x.ReferenceId == referenceId && x.ReferenceType == referenceType),
                cancellationToken);

        var paymentIntentId = GetSessionValue(checkoutSession, "PaymentIntent") ?? checkoutSession.Id;
        var targetStatus = string.Equals(eventType, "checkout.session.expired", StringComparison.OrdinalIgnoreCase)
            ? PaymentStatus.Cancelled
            : PaymentStatus.Succeeded;

        if (payment is null)
        {
            payment = Payment.Create(
                referenceId,
                referenceType,
                GetSessionAmount(checkoutSession),
                checkoutSession.Currency ?? "usd",
                PaymentProvider.Stripe,
                paymentIntentId,
                null,
                targetStatus,
                null);
            _context.Add(payment);
        }
        else
        {
            payment.UpdateGatewayState(paymentIntentId, null, targetStatus);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Payment?> FindByMetadataAsync(IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        var referenceId = GetMetadata(metadata, "reference_id", string.Empty);
        var referenceType = GetMetadata(metadata, "reference_type", string.Empty);

        if (string.IsNullOrWhiteSpace(referenceId) || string.IsNullOrWhiteSpace(referenceType))
        {
            return null;
        }

        return await _context.Payments.SingleOrDefaultAsync(
            x => x.ReferenceId == referenceId && x.ReferenceType == referenceType,
            cancellationToken);
    }

    private static string GetMetadata(PaymentIntent paymentIntent, string key, string fallback)
    {
        return paymentIntent.Metadata != null &&
               paymentIntent.Metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static string GetMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        return metadata != null &&
               metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static string? GetSessionValue(Session session, string propertyName)
    {
        return session.GetType().GetProperty(propertyName)?.GetValue(session)?.ToString();
    }

    private static decimal GetSessionAmount(Session session)
    {
        var amount = session.GetType().GetProperty("AmountTotal")?.GetValue(session);

        return amount switch
        {
            long cents => cents / 100m,
            int cents => cents / 100m,
            _ => 0m
        };
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
