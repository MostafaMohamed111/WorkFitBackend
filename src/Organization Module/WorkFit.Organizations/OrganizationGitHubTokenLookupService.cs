using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationGitHub;
using WorkFit.Organizations.Infrastructure.Data;

namespace WorkFit.Organizations;

public sealed class OrganizationGitHubInstallationLookupService : IGitHubOrganizationInstallationLookupService
{
    private readonly OrganizationDbContext _context;

    public OrganizationGitHubInstallationLookupService(OrganizationDbContext context) => _context = context;

    public async Task<long?> GetGitHubInstallationIdForOrganizationAsync(string organizationLogin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(organizationLogin))
        {
            return null;
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.GitHubOrganizationLogin == organizationLogin.Trim(), cancellationToken);

        if (organization is null)
        {
            return null;
        }

        var installation = await _context.GitHubAppInstallations
            .FirstOrDefaultAsync(x => x.OrganizationId == organization.Id, cancellationToken);

        return installation?.GitHubInstallationId;
    }
}
