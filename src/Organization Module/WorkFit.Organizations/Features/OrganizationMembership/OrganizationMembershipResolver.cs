using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationMembership;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;

namespace WorkFit.Organizations.Features.OrganizationMembership;

internal sealed class OrganizationMembershipResolver : IOrganizationMembershipResolver
{
    private readonly OrganizationDbContext _db;

    public OrganizationMembershipResolver(OrganizationDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> GetOrganizationIdForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orgId = await _db.Organizations
            .Where(o => o.UserId == userId || o.Members.Any(m => m.UserId == userId))
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return orgId ?? throw new OrganizationNotFoundException();
    }
}