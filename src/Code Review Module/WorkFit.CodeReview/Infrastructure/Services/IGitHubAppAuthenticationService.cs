namespace WorkFit.CodeReview.Infrastructure.Services;

public interface IGitHubAppAuthenticationService
{
    Task<string> GetInstallationAccessTokenAsync(long installationId, CancellationToken cancellationToken = default);
}
