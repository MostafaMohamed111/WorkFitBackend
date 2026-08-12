using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkFit.CodeReview.Infrastructure.Options;
using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Features.OrganizationsMe;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/github/webhook")]
public sealed class GitHubWebhookController : ControllerBase
{
    private const string SupportedPushEvent = "push";
    private const string SupportedPullRequestEvent = "pull_request";
    private const string SupportedInstallationEvent = "installation";
    private const string PayloadItemKey = "GitHubWebhook:PullRequestPayload";

    private readonly IOptions<CodeReviewOptions> _codeReviewOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMediator _mediator;
    private readonly ILogger<GitHubWebhookController> _logger;

    public GitHubWebhookController(
        IOptions<CodeReviewOptions> codeReviewOptions,
        IHttpClientFactory httpClientFactory,
        IMediator mediator,
        ILogger<GitHubWebhookController> logger)
    {
        _codeReviewOptions = codeReviewOptions;
        _httpClientFactory = httpClientFactory;
        _mediator = mediator;
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

        if (string.Equals(eventType, SupportedInstallationEvent, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleInstallationEventAsync(payload, cancellationToken);
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

    private async Task<IActionResult> HandleInstallationEventAsync(byte[] payload, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(payload);

        if (!TryReadInstallationPayload(document.RootElement, out var installationData, out var action))
        {
            return BadRequest("Invalid GitHub installation payload.");
        }

        if (!string.Equals(action, "created", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Ignoring GitHub installation event action {Action} for organization {OrganizationLogin}.",
                action,
                installationData.GitHubOrganizationLogin);
            return Ok();
        }

        var installedAt = await TryGetInstallationCreatedAtAsync(
            installationData.GitHubInstallationId,
            cancellationToken) ?? installationData.InstalledAt;

        try
        {
            await _mediator.Send(
                new UpsertOrganizationGitHubFromWebhookCommand(
                    installationData.GitHubOrganizationId,
                    installationData.GitHubOrganizationLogin,
                    installationData.GitHubCreatedAt,
                    installationData.GitHubInstallationId,
                    installedAt),
                cancellationToken);
        }
        catch (OrganizationNotFoundException)
        {
            _logger.LogWarning(
                "No organization matched GitHub login {OrganizationLogin} from installation webhook.",
                installationData.GitHubOrganizationLogin);
        }

        return Ok();
    }

    private static bool TryReadInstallationPayload(
        JsonElement root,
        out GitHubInstallationWebhookData installationData,
        out string? action)
    {
        installationData = default!;
        action = null;

        if (!TryGetString(root, "action", out action))
        {
            return false;
        }

        if (!TryGetObject(root, "installation", out var installation))
        {
            return false;
        }

        if (!TryGetLong(installation, "id", out var installationId))
        {
            return false;
        }

        if (!TryGetObject(root, "organization", out var organization))
        {
            if (!TryGetObject(installation, "account", out organization))
            {
                if (!TryGetObject(root, "account", out organization))
                {
                    return false;
                }
            }
        }

        if (!TryGetLong(organization, "id", out var organizationId))
        {
            return false;
        }

        if (!TryGetString(organization, "login", out var organizationLogin))
        {
            return false;
        }

        var installedAt = TryGetDateTimeOffset(installation, "created_at", out var installationCreatedAt)
            ? installationCreatedAt
            : DateTimeOffset.UtcNow;

        DateTimeOffset? githubCreatedAt = null;
        if (TryGetDateTimeOffset(organization, "created_at", out var accountCreatedAt))
        {
            githubCreatedAt = accountCreatedAt;
        }
        else if (TryGetNestedDateTimeOffset(root, "organization", "created_at", out var organizationCreatedAt))
        {
            githubCreatedAt = organizationCreatedAt;
        }

        installationData = new GitHubInstallationWebhookData(
            organizationId,
            organizationLogin,
            githubCreatedAt,
            installationId,
            installedAt);

        return true;
    }

    private static bool TryGetObject(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out value) &&
            value.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    private static bool TryGetLong(JsonElement element, string propertyName, out long value)
    {
        value = default;

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.TryGetInt64(out value))
        {
            return true;
        }

        return false;
    }

    private async Task<DateTimeOffset?> TryGetInstallationCreatedAtAsync(long installationId, CancellationToken cancellationToken)
    {
        var githubOptions = _codeReviewOptions.Value.GitHub;
        if (string.IsNullOrWhiteSpace(githubOptions.AppId) || string.IsNullOrWhiteSpace(githubOptions.AppPrivateKey))
        {
            _logger.LogWarning("GitHub App credentials are not configured, falling back to webhook timestamp for installation {InstallationId}.", installationId);
            return null;
        }

        try
        {
            var jwt = CreateJwt(githubOptions.AppId, githubOptions.AppPrivateKey);
            var client = _httpClientFactory.CreateClient("CodeReviewGitHub");
            client.BaseAddress ??= new Uri(githubOptions.BaseUrl, UriKind.Absolute);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"app/installations/{installationId}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", githubOptions.ApiVersion);
            request.Headers.TryAddWithoutValidation("User-Agent", githubOptions.UserAgent);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Failed to fetch GitHub installation {InstallationId}. Status={StatusCode}. Body={Body}",
                    installationId,
                    (int)response.StatusCode,
                    body);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("created_at", out var createdAtElement) &&
                createdAtElement.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(createdAtElement.GetString(), out var createdAt))
            {
                return createdAt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve GitHub installation created_at for installation {InstallationId}.", installationId);
        }

        return null;
    }

    private static string CreateJwt(string appId, string privateKeyPem)
    {
        var privateKey = PemKeyUtils.GetRsaSecurityKey(privateKeyPem);
        var now = DateTimeOffset.UtcNow;
        var signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: appId,
            audience: "https://api.github.com/",
            claims: new[]
            {
                new Claim("iat", now.ToUnixTimeSeconds().ToString()),
                new Claim("exp", now.AddMinutes(9).ToUnixTimeSeconds().ToString())
            },
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(9).UtcDateTime,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool TryGetDateTimeOffset(JsonElement element, string propertyName, out DateTimeOffset value)
    {
        value = default;

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetNestedDateTimeOffset(
        JsonElement element,
        string parentPropertyName,
        string propertyName,
        out DateTimeOffset value)
    {
        value = default;

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(parentPropertyName, out var parent) &&
            parent.ValueKind == JsonValueKind.Object &&
            parent.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private sealed record GitHubInstallationWebhookData(
        long GitHubOrganizationId,
        string GitHubOrganizationLogin,
        DateTimeOffset? GitHubCreatedAt,
        long GitHubInstallationId,
        DateTimeOffset InstalledAt);

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
