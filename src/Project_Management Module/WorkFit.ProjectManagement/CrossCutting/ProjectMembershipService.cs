using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.Membership;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Infrastructure;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class ProjectMembershipService : IProjectMembershipService
{
    private readonly WorkFitProjectDbContext _db;
    public ProjectMembershipService(WorkFitProjectDbContext db) => _db = db;

    public Task<ProjectInvitationScope?> GetInvitationScopeAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _db.Projects.AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectInvitationScope(p.Id, p.OrganizationId, p.TeamLeaderId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> IsTeamLeaderInOrganizationAsync(
        Guid teamLeaderId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        _db.Projects.AsNoTracking().AnyAsync(
            p => p.TeamLeaderId == teamLeaderId && p.OrganizationId == organizationId,
            cancellationToken);

    public async Task AddMemberAsync(Guid projectId, Guid employeeProfileId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var validProject = await _db.Projects.AnyAsync(p => p.Id == projectId && p.OrganizationId == organizationId, cancellationToken);
        if (!validProject) throw new InvalidOperationException("Project does not belong to the invitation organization.");

        if (await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.EmployeeProfileId == employeeProfileId, cancellationToken)) return;
        _db.ProjectMembers.Add(ProjectMember.Create(projectId, employeeProfileId));
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetMemberIdsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectMembers.AsNoTracking()
            .Where(member => member.ProjectId == projectId)
            .Select(member => member.EmployeeProfileId)
            .ToListAsync(cancellationToken);
}
