using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WorkFit.CodeReview.Infrastructure.Options;

namespace WorkFit.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/github/webhook")]
public sealed class GitHubWebhookController : ControllerBase
{
    private const string SupportedPushEvent = "push";
    private const string SupportedPullRequestEvent = "pull_request";
    private const string PayloadItemKey = "GitHubWebhook:PullRequestPayload";

    private readonly IOptions<CodeReviewOptions> _codeReviewOptions;
    private readonly ILogger<GitHubWebhookController> _logger;

    public GitHubWebhookController(
        IOptions<CodeReviewOptions> codeReviewOptions,
        ILogger<GitHubWebhookController> logger)
    {
        _codeReviewOptions = codeReviewOptions;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveAsync(CancellationToken cancellationToken)
    {
        var eventType = Request.Headers["X-GitHub-Event"].ToString();
        var deliveryId = Request.Headers["X-GitHub-Delivery"].ToString();
        var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return BadRequest("Missing X-GitHub-Event header.");
        }

        if (string.IsNullOrWhiteSpace(deliveryId))
        {
            return BadRequest("Missing X-GitHub-Delivery header.");
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return Unauthorized();
        }

        var webhookSecret = _codeReviewOptions.Value.GitHub.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new InvalidOperationException("CodeReview:GitHub:WebhookSecret is not configured.");
        }

        Request.EnableBuffering();
        byte[] payload;

        using (var memoryStream = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memoryStream, cancellationToken);
            payload = memoryStream.ToArray();
        }

        Request.Body.Position = 0;

        if (!IsValidSignature(payload, signatureHeader, webhookSecret))
        {
            _logger.LogWarning(
                "Rejected GitHub webhook with invalid signature. EventType={EventType}, DeliveryId={DeliveryId}.",
                eventType,
                deliveryId);

            return Unauthorized();
        }

        _logger.LogInformation(
            "Received GitHub webhook event {EventType}. DeliveryId={DeliveryId}.",
            eventType,
            deliveryId);

        if (string.Equals(eventType, SupportedPullRequestEvent, StringComparison.OrdinalIgnoreCase))
        {
            HttpContext.Items[PayloadItemKey] = Encoding.UTF8.GetString(payload);
        }

        if (!string.Equals(eventType, SupportedPushEvent, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(eventType, SupportedPullRequestEvent, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "GitHub webhook event {EventType} is not currently handled. DeliveryId={DeliveryId}.",
                eventType,
                deliveryId);
        }

        return Ok();
    }

    private static bool IsValidSignature(byte[] payload, string signatureHeader, string webhookSecret)
    {
        const string prefix = "sha256=";

        if (!signatureHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var providedSignatureHex = signatureHeader[prefix.Length..].Trim();
        if (providedSignatureHex.Length != 64)
        {
            return false;
        }

        byte[] providedSignature;
        try
        {
            providedSignature = Convert.FromHexString(providedSignatureHex);
        }
        catch (FormatException)
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
        var calculatedSignature = hmac.ComputeHash(payload);

        return CryptographicOperations.FixedTimeEquals(calculatedSignature, providedSignature);
    }
}
