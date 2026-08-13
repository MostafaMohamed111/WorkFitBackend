using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.Invitations;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class DeveloperInvitationTalentService : IDeveloperInvitationTalentService
{
    private readonly TalentDbContext _db;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public DeveloperInvitationTalentService(TalentDbContext db, EmployeeIndexingStatePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<PendingDeveloperDto?> GetPendingDeveloperAsync(Guid organizationId, Guid employeeProfileId, string sourceSystem, string sourceAccountId, CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeProfiles.AsNoTracking()
            .Where(e => e.Id == employeeProfileId && e.OrganizationId == organizationId && e.UserId == Guid.Empty)
            .Where(e => e.IdentityMappings.Any(m => m.OrganizationId == organizationId && m.SourceSystem == sourceSystem && m.ExternalAccountId == sourceAccountId))
            .Select(e => new PendingDeveloperDto(e.Id, e.Name, e.Email))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task LinkAndActivateAsync(Guid organizationId, Guid employeeProfileId, Guid userId, string displayName, string email, CancellationToken cancellationToken = default)
    {
        var employee = await _db.EmployeeProfiles.SingleOrDefaultAsync(e => e.Id == employeeProfileId && e.OrganizationId == organizationId, cancellationToken)
            ?? throw new InvalidOperationException("Pending employee profile was not found in the invitation organization.");

        employee.LinkUser(userId, displayName, email);
        await _db.SaveChangesAsync(cancellationToken);
        await _publisher.PublishAsync(employee.Id, "Updated", cancellationToken);
    }
}
