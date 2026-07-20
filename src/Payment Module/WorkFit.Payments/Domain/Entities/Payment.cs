using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Domain.Entities;

public sealed class Payment
{
    private Payment()
    {
    }

    public Guid Id { get; private set; }

    public string ReferenceId { get; private set; } = string.Empty;

    public string ReferenceType { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public PaymentStatus Status { get; private set; }

    public PaymentProvider Provider { get; private set; }

    public string ProviderPaymentId { get; private set; } = string.Empty;

    public string? TransactionId { get; private set; }

    public string? ClientSecret { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Payment Create(
        string referenceId,
        string referenceType,
        decimal amount,
        string currency,
        PaymentProvider provider,
        string providerPaymentId,
        string? transactionId,
        PaymentStatus status,
        string? clientSecret)
    {
        var now = DateTimeOffset.UtcNow;

        return new Payment
        {
            Id = Guid.NewGuid(),
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Amount = amount,
            Currency = currency,
            Provider = provider,
            ProviderPaymentId = providerPaymentId,
            TransactionId = transactionId,
            Status = status,
            ClientSecret = clientSecret,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateGatewayState(
        string providerPaymentId,
        string? transactionId,
        PaymentStatus status,
        string? clientSecret = null)
    {
        ProviderPaymentId = providerPaymentId;
        TransactionId = transactionId ?? TransactionId;
        Status = status;

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            ClientSecret = clientSecret;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkStatus(PaymentStatus status, string? transactionId = null)
    {
        Status = status;

        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            TransactionId = transactionId;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
