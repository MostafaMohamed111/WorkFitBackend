using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed record PaymentGatewayResult(
    string ProviderPaymentId,
    string? TransactionId,
    PaymentStatus Status,
    string? ClientSecret,
    decimal Amount,
    string Currency,
    string? FailureReason = null);
