using FastEndpoints;
using Stripe;
using WorkFit.Payments.Infrastructure.Configuration;
using WorkFit.Payments.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace WorkFit.Payments.Features.Payments.Webhook;

public sealed class PaymentWebhookEndpoint : EndpointWithoutRequest
{
    private readonly IPaymentWebhookService _webhookService;
    private readonly IOptions<PaymentOptions> _paymentOptions;

    public PaymentWebhookEndpoint(
        IPaymentWebhookService webhookService,
        IOptions<PaymentOptions> paymentOptions)
    {
        _webhookService = webhookService;
        _paymentOptions = paymentOptions;
    }

    public override void Configure()
    {
        Post("/api/payments/webhook");
        AllowAnonymous();
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces(200)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!string.Equals(_paymentOptions.Value.Provider, PaymentProviderName.Stripe, StringComparison.OrdinalIgnoreCase))
        {
            await Send.OkAsync(cancellation: ct);
            return;
        }

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = HttpContext.Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _paymentOptions.Value.Stripe.WebhookSecret;

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            ThrowError("Stripe webhook secret is missing.");
        }

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            ThrowError(ex.Message);
            return;
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.created":
            case "payment_intent.succeeded":
            case "payment_intent.payment_failed":
            case "payment_intent.canceled":
                await _webhookService.HandleEventAsync(stripeEvent, ct);
                break;
        }

        await Send.OkAsync(cancellation: ct);
    }
}
