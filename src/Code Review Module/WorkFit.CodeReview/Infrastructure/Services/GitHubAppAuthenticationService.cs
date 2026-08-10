using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkFit.CodeReview.Infrastructure.Options;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class GitHubAppAuthenticationService : IGitHubAppAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<CodeReviewOptions> _options;
    private readonly ILogger<GitHubAppAuthenticationService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<long, (string Token, DateTimeOffset ExpiresAt)> _tokenCache = new();

    public GitHubAppAuthenticationService(
        IHttpClientFactory httpClientFactory,
        IOptions<CodeReviewOptions> options,
        ILogger<GitHubAppAuthenticationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetInstallationAccessTokenAsync(long installationId, CancellationToken cancellationToken = default)
    {
        var options = _options.Value.GitHub;
        if (string.IsNullOrWhiteSpace(options.AppId))
        {
            throw new InvalidOperationException("GitHub AppId is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.AppPrivateKey))
        {
            throw new InvalidOperationException("GitHub App private key is not configured.");
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_tokenCache.TryGetValue(installationId, out var cache) && cache.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return cache.Token;
            }

            var jwt = CreateJwt(options.AppId, options.AppPrivateKey);
            var client = _httpClientFactory.CreateClient("CodeReviewGitHub");
            client.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"app/installations/{installationId}/access_tokens");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", options.ApiVersion);
            request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var token = document.RootElement.GetProperty("token").GetString();
            var expiresAt = document.RootElement.GetProperty("expires_at").GetDateTimeOffset();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("GitHub App installation token response did not contain a token.");
            }

            _tokenCache[installationId] = (token, expiresAt);
            return token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string CreateJwt(string appId, string privateKeyPem)
    {
        var privateKey = PemKeyUtils.GetRsaSecurityKey(privateKeyPem);
        var now = DateTimeOffset.UtcNow;
        var signingCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: appId,
            audience: "https://api.github.com/",
            claims: new[] { new Claim("iat", ((DateTimeOffset)now).ToUnixTimeSeconds().ToString()), new Claim("exp", ((DateTimeOffset)now.AddMinutes(9)).ToUnixTimeSeconds().ToString()) },
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(9).UtcDateTime,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
