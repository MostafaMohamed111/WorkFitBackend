using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WorkFit.Payments.Infrastructure.Configuration;

namespace WorkFit.Payments.Infrastructure.Services;

public sealed class StripeWebhookProcessor : IStripeWebhookProcessor
{
    private static readonly HashSet<string> SupportedEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "payment_intent.created",
        "payment_intent.succeeded",
        "payment_intent.payment_failed",
        "payment_intent.canceled",
        "payment_intent.processing",
        "checkout.session.completed",
        "checkout.session.expired"
    };

    private readonly IPaymentWebhookService _paymentWebhookService;
    private readonly IOptions<PaymentOptions> _paymentOptions;
    private readonly ILogger<StripeWebhookProcessor> _logger;

    public StripeWebhookProcessor(
        IPaymentWebhookService paymentWebhookService,
        IOptions<PaymentOptions> paymentOptions,
        ILogger<StripeWebhookProcessor> logger)
    {
        _paymentWebhookService = paymentWebhookService;
        _paymentOptions = paymentOptions;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!string.Equals(_paymentOptions.Value.Provider, PaymentProviderName.Stripe, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        httpContext.Request.EnableBuffering();
        string json;

        using (var reader = new StreamReader(
                   httpContext.Request.Body,
                   leaveOpen: true))
        {
            json = await reader.ReadToEndAsync(cancellationToken);
        }

        httpContext.Request.Body.Position = 0;

        var signature = httpContext.Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _paymentOptions.Value.Stripe.WebhookSecret;

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new InvalidOperationException("Stripe-Signature header is missing.");
        }

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new InvalidOperationException(
                "Stripe webhook secret is missing. Set Payments:Stripe:WebhookSecret to the current Stripe CLI or Dashboard secret.");
        }

        Event stripeEvent;
        var webhookSecrets = GetCandidateWebhookSecrets(webhookSecret);
        StripeException? lastStripeException = null;

        foreach (var candidateSecret in webhookSecrets)
        {
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    candidateSecret,
                    throwOnApiVersionMismatch: false);
                goto EventValidated;
            }
            catch (StripeException ex)
            {
                lastStripeException = ex;

                if (IsApiVersionMismatch(ex))
                {
                    _logger.LogWarning(
                        ex,
                        "Stripe webhook event API version is newer than the installed Stripe.net SDK supports. ContentType={ContentType}, PayloadLength={PayloadLength}. Consider updating the webhook endpoint API version in Stripe to match the SDK, or upgrading Stripe.net.",
                        httpContext.Request.ContentType,
                        json.Length);
                    return;
                }

                _logger.LogWarning(
                    ex,
                    "Stripe webhook signature validation failed using one configured secret. ContentType={ContentType}, PayloadLength={PayloadLength}.",
                    httpContext.Request.ContentType,
                    json.Length);
            }
        }

        throw new InvalidOperationException(
            lastStripeException?.Message
            ?? "Stripe webhook signature validation failed. Check the current Stripe webhook secret.");

    EventValidated:

        if (!SupportedEventTypes.Contains(stripeEvent.Type))
        {
            return;
        }

        await _paymentWebhookService.HandleEventAsync(stripeEvent, cancellationToken);
    }

    private static IReadOnlyList<string> GetCandidateWebhookSecrets(string configuredSecret)
    {
        return configuredSecret
            .Split(new[] { ',', ';', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsApiVersionMismatch(StripeException exception)
    {
        return exception.Message.Contains("Received event with API version", StringComparison.OrdinalIgnoreCase);
    }
}
