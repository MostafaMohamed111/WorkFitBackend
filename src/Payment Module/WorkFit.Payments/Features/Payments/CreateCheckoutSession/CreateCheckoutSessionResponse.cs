using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Features.Payments.CreateCheckoutSession;

public sealed record CreateCheckoutSessionResponse(
    Guid PaymentId,
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
    DateTimeOffset UpdatedAt,
    string CheckoutSessionId,
    string CheckoutUrl);
