using Stripe;

namespace WorkFit.Payments.Infrastructure.Services;

public interface IPaymentWebhookService
{
    Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken);
}
