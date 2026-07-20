using Microsoft.Extensions.Options;
using Stripe;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Infrastructure.Configuration;

namespace WorkFit.Payments.Infrastructure.Gateways;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly PaymentIntentService _paymentIntentService;

    public StripePaymentGateway(IOptions<PaymentOptions> options)
    {
        var stripeOptions = options.Value.Stripe;

        if (string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
        {
            throw new InvalidOperationException("Payments:Stripe:SecretKey is required.");
        }

        var client = new StripeClient(stripeOptions.SecretKey);
        _paymentIntentService = new PaymentIntentService(client);
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

    public async Task<PaymentGatewayResult> RetrievePaymentIntentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _paymentIntentService.GetAsync(
            providerPaymentId,
            cancellationToken: cancellationToken);

        return Map(paymentIntent);
    }

    public async Task<PaymentGatewayResult> ConfirmPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _paymentIntentService.ConfirmAsync(
            providerPaymentId,
            new PaymentIntentConfirmOptions(),
            cancellationToken: cancellationToken);

        return Map(paymentIntent);
    }

    public async Task<PaymentGatewayResult> CancelPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        var paymentIntent = await _paymentIntentService.CancelAsync(
            providerPaymentId,
            new PaymentIntentCancelOptions(),
            cancellationToken: cancellationToken);

        return Map(paymentIntent);
    }

    public async Task<PaymentGatewayResult> GetPaymentStatusAsync(
        string providerPaymentId,
        CancellationToken cancellationToken)
    {
        return await RetrievePaymentIntentAsync(providerPaymentId, cancellationToken);
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
