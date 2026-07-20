namespace WorkFit.Payments.Infrastructure.Configuration;

public sealed class PaymentOptions
{
    public string Provider { get; set; } = PaymentProviderName.Mock;

    public StripeOptions Stripe { get; set; } = new();
}
