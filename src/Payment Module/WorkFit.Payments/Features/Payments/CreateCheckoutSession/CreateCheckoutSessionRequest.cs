namespace WorkFit.Payments.Features.Payments.CreateCheckoutSession;

public sealed class CreateCheckoutSessionRequest
{
    public string ReferenceId { get; set; } = string.Empty;

    public string ReferenceType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "usd";

    public string? Description { get; set; }
}
