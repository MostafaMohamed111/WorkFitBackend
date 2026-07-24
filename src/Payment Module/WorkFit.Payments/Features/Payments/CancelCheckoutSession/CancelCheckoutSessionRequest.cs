namespace WorkFit.Payments.Features.Payments.CancelCheckoutSession;

public sealed class CancelCheckoutSessionRequest
{
    public string ReferenceId { get; set; } = string.Empty;

    public string ReferenceType { get; set; } = string.Empty;
}
