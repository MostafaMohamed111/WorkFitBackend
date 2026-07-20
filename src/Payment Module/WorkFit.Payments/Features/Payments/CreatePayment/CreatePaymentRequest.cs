using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Features.Payments.CreatePayment;

public sealed class CreatePaymentRequest
{
    public string ReferenceId { get; set; } = string.Empty;

    public string ReferenceType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "usd";

    public string? Description { get; set; }

    public MockPaymentOutcome? MockOutcome { get; set; }
}
