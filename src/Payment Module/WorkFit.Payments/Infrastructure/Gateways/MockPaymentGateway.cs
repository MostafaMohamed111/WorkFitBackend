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

    public Task<PaymentGatewayResult> RetrievePaymentIntentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(GetStoredResult(providerPaymentId));
    }

    public Task<PaymentGatewayResult> ConfirmPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var current = GetStoredResult(providerPaymentId);
        var confirmed = current.Status is PaymentStatus.Cancelled or PaymentStatus.Failed
            ? current
            : current with
            {
                Status = PaymentStatus.Succeeded,
                TransactionId = current.TransactionId ?? $"txn_mock_{Guid.NewGuid():N}"
            };

        Store[providerPaymentId] = confirmed;
        return Task.FromResult(confirmed);
    }

    public Task<PaymentGatewayResult> CancelPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var current = GetStoredResult(providerPaymentId);
        var cancelled = current with
        {
            Status = PaymentStatus.Cancelled,
            TransactionId = current.TransactionId ?? $"txn_mock_{Guid.NewGuid():N}"
        };

        Store[providerPaymentId] = cancelled;
        return Task.FromResult(cancelled);
    }

    public Task<PaymentGatewayResult> GetPaymentStatusAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        return RetrievePaymentIntentAsync(providerPaymentId, cancellationToken);
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
