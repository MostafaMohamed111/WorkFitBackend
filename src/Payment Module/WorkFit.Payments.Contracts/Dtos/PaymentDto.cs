using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Contracts.Dtos;

public sealed record PaymentDto(
    Guid Id,
    string ReferenceId,
    string ReferenceType,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    PaymentProvider Provider,
    string ProviderPaymentId,
    string? TransactionId,
    string? ClientSecret,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
