using System.Collections.Concurrent;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Infrastructure.Configuration;

namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed class MockPaymentGateway : IPaymentGateway
{
    private static readonly ConcurrentDictionary<string, PaymentGatewayResult> Store = new();

    public PaymentProvider Provider => PaymentProvider.Mock;

    public Task<PaymentGatewayResult> CreatePaymentIntentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = request.MockOutcome ?? MockPaymentOutcome.Success;
        var providerPaymentId = $"pi_mock_{Guid.NewGuid():N}";
        var transactionId = outcome == MockPaymentOutcome.Success
            ? $"txn_mock_{Guid.NewGuid():N}"
            : null;
        var status = outcome switch
        {
            MockPaymentOutcome.Success => PaymentStatus.Succeeded,
            MockPaymentOutcome.Failed => PaymentStatus.Failed,
            MockPaymentOutcome.Pending => PaymentStatus.Pending,
            MockPaymentOutcome.Cancelled => PaymentStatus.Cancelled,
            _ => PaymentStatus.Pending
        };
        var result = new PaymentGatewayResult(
            providerPaymentId,
            transactionId,
            status,
            $"cs_mock_{Guid.NewGuid():N}",
            request.Amount,
            request.Currency);

        Store[providerPaymentId] = result;
        return Task.FromResult(result);
    }

    public Task<PaymentCheckoutSessionResult> CreateCheckoutSessionAsync(
        PaymentGatewayRequest request,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken)
    {
        var sessionId = $"cs_mock_{Guid.NewGuid():N}";
        var url = successUrl.Replace("{CHECKOUT_SESSION_ID}", sessionId, StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(new PaymentCheckoutSessionResult(sessionId, url));
    }

    public Task<PaymentGatewayResult> RetrievePaymentIntentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(GetStoredResult(providerPaymentId));
    }

    private static PaymentGatewayResult GetStoredResult(string providerPaymentId)
    {
        if (Store.TryGetValue(providerPaymentId, out var result))
        {
            return result;
        }

        return new PaymentGatewayResult(
            providerPaymentId,
            $"txn_mock_{Guid.NewGuid():N}",
            PaymentStatus.Pending,
            $"cs_mock_{Guid.NewGuid():N}",
            0m,
            "usd",
            "Mock payment intent not found in memory.");
    }
}
