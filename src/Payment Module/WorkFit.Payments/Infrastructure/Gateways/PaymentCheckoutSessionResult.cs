namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed record PaymentCheckoutSessionResult(
    string SessionId,
    string Url);
