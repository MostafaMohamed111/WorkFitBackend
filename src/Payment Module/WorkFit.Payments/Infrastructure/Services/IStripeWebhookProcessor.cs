using Microsoft.AspNetCore.Http;

namespace WorkFit.Payments.Infrastructure.Services;

public interface IStripeWebhookProcessor
{
    Task HandleAsync(HttpContext httpContext, CancellationToken cancellationToken);
}
