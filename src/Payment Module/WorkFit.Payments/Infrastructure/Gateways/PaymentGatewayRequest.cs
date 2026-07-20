using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed record PaymentGatewayRequest(
    decimal Amount,
    string Currency,
    string ReferenceId,
    string ReferenceType,
    string? Description,
    IReadOnlyDictionary<string, string> Metadata,
    MockPaymentOutcome? MockOutcome);
