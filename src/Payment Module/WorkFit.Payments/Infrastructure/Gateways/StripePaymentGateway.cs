using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Infrastructure.Configuration;

namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly PaymentIntentService _paymentIntentService;
    private readonly SessionService _checkoutSessionService;

    public StripePaymentGateway(IOptions<PaymentOptions> options)
    {
        var stripeOptions = options.Value.Stripe;

        if (string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
        {
            throw new InvalidOperationException("Payments:Stripe:SecretKey is required.");
        }

        var client = new StripeClient(stripeOptions.SecretKey);
        _paymentIntentService = new PaymentIntentService(client);
        _checkoutSessionService = new SessionService(client);
    }

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public async Task<PaymentGatewayResult> CreatePaymentIntentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _paymentIntentService.CreateAsync(
            new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(request.Amount),
                Currency = request.Currency,
                Description = request.Description,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>(request.Metadata)
            },
            cancellationToken: cancellationToken);

        return Map(paymentIntent);
    }

    public async Task<PaymentCheckoutSessionResult> CreateCheckoutSessionAsync(
        PaymentGatewayRequest request,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken)
    {
        var session = await _checkoutSessionService.CreateAsync(
            new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Currency,
                            UnitAmount = ToStripeAmount(request.Amount),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.Description ?? "WorkFit payment",
                                Description = $"{request.ReferenceType} {request.ReferenceId}",
                                Metadata = new Dictionary<string, string>(request.Metadata)
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string>(request.Metadata),
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>(request.Metadata)
                }
            },
            cancellationToken: cancellationToken);

        return new PaymentCheckoutSessionResult(
            session.Id,
            session.Url ?? successUrl);
    }

    public async Task<PaymentGatewayResult> RetrievePaymentIntentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _paymentIntentService.GetAsync(
            providerPaymentId,
            cancellationToken: cancellationToken);

        return Map(paymentIntent);
    }

    private static PaymentGatewayResult Map(PaymentIntent paymentIntent)
    {
        return new PaymentGatewayResult(
            paymentIntent.Id,
            GetLatestChargeId(paymentIntent),
            MapStatus(paymentIntent.Status),
            paymentIntent.ClientSecret,
            paymentIntent.Amount / 100m,
            paymentIntent.Currency ?? "usd");
    }

    private static long ToStripeAmount(decimal amount)
    {
        return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }

    private static PaymentStatus MapStatus(string? stripeStatus)
    {
        return stripeStatus?.ToLowerInvariant() switch
        {
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Cancelled,
            "requires_payment_method" => PaymentStatus.Failed,
            "requires_action" => PaymentStatus.Pending,
            "processing" => PaymentStatus.Pending,
            "requires_confirmation" => PaymentStatus.Pending,
            _ => PaymentStatus.Pending
        };
    }

    private static string? GetLatestChargeId(PaymentIntent paymentIntent)
    {
        var latestChargeIdProperty = paymentIntent.GetType().GetProperty("LatestChargeId");
        var latestChargeProperty = paymentIntent.GetType().GetProperty("LatestCharge");

        return latestChargeIdProperty?.GetValue(paymentIntent)?.ToString()
            ?? latestChargeProperty?.GetValue(paymentIntent)?.ToString();
    }
}
