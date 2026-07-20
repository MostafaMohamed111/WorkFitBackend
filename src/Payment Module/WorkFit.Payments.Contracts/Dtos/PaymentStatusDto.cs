using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Contracts.Dtos;

public sealed record PaymentStatusDto(
    Guid Id,
    PaymentStatus Status,
    PaymentProvider Provider,
    string ProviderPaymentId,
    string? TransactionId,
    DateTimeOffset UpdatedAt);
