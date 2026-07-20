using WorkFit.Payments.Contracts.Enums;

namespace WorkFit.Payments.Infrastructure.Gateways;

public interface IPaymentGateway
{
    PaymentProvider Provider { get; }

    Task<PaymentGatewayResult> CreatePaymentIntentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken);

    Task<PaymentGatewayResult> RetrievePaymentIntentAsync(string providerPaymentId, CancellationToken cancellationToken);

    Task<PaymentGatewayResult> ConfirmPaymentAsync(string providerPaymentId, CancellationToken cancellationToken);

    Task<PaymentGatewayResult> CancelPaymentAsync(string providerPaymentId, CancellationToken cancellationToken);

    Task<PaymentGatewayResult> GetPaymentStatusAsync(string providerPaymentId, CancellationToken cancellationToken);
}
