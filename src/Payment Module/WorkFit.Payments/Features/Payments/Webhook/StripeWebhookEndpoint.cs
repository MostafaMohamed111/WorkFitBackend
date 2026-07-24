using FastEndpoints;
using WorkFit.Payments.Infrastructure.Services;

namespace WorkFit.Payments.Features.Payments.Webhook;

[HideFromDocs]
public sealed class StripeWebhookEndpoint : StripeWebhookEndpointBase
{
    public StripeWebhookEndpoint(IStripeWebhookProcessor webhookProcessor)
        : base(webhookProcessor)
    {
    }

    protected override string RouteTemplate => "/api/stripe/webhook";
}
