using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationGitHub;
using WorkFit.Organizations.Infrastructure.Data;

namespace WorkFit.Organizations.CrossModule.GitHubOrganizationLogin;

internal sealed class GitHubOrganizationLoginLookupService : IGitHubOrganizationLoginLookupService
{
    private readonly OrganizationDbContext _context;

    public GitHubOrganizationLoginLookupService(OrganizationDbContext context) => _context = context;

    public async Task<string?> GetGitHubOrganizationLoginAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            return null;
        }

        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);

        return organization?.GitHubOrganizationLogin;
    }
}