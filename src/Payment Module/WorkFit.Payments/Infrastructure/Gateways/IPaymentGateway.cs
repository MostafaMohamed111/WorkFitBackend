using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Infrastructure.Gateways;

public interface IPaymentGateway
{
    PaymentProvider Provider { get; }

    Task<PaymentGatewayResult> CreatePaymentIntentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken);

    Task<PaymentCheckoutSessionResult> CreateCheckoutSessionAsync(
        PaymentGatewayRequest request,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken);

    Task<PaymentGatewayResult> RetrievePaymentIntentAsync(string providerPaymentId, CancellationToken cancellationToken);
}
