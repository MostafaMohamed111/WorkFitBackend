using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Payments.Infrastructure.Services;

namespace WorkFit.Payments.Features.Payments.Webhook;

public abstract class StripeWebhookEndpointBase : EndpointWithoutRequest
{
    private readonly IStripeWebhookProcessor _webhookProcessor;

    protected StripeWebhookEndpointBase(IStripeWebhookProcessor webhookProcessor)
    {
        _webhookProcessor = webhookProcessor;
    }

    protected abstract string RouteTemplate { get; }

    public override void Configure()
    {
        Post(RouteTemplate);
        AllowAnonymous();
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces(200)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            await _webhookProcessor.HandleAsync(HttpContext, ct);
        }
        catch (InvalidOperationException ex)
        {
            ThrowError(ex.Message);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}
