namespace WorkFit.Organizations.Contracts.OrganizationGitHub;

public interface IGitHubOrganizationInstallationLookupService
{
    Task<long?> GetGitHubInstallationIdForOrganizationAsync(string organizationLogin, CancellationToken cancellationToken = default);
}
