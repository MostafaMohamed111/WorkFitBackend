namespace WorkFit.Organizations.Contracts.OrganizationGitHub;

public interface IGitHubOrganizationLoginLookupService
{
    Task<string?> GetGitHubOrganizationLoginAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}