using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.ProjectManagement.Features.Common;

/// <summary>
/// Shared authorization gate for project-level commands/queries.
/// The caller must either be the project's team lead (by UserId or EmployeeProfileId),
/// hold a management role (OrganizationOwner, Admin, SuperAdmin),
/// or be a TeamLeader within the project's organization.
/// </summary>
public static class ProjectAccessGuard
{
    public static void EnsureAuthorized(Domain.Entities.Project project, ICurrentUserContext currentUser, CancellationToken ct)
    {
        var actorId = currentUser.GetUserId(ct);
        var roles = currentUser.GetRoles(ct);

        var isSuperAdmin = roles.Contains("SuperAdmin");
        var isOrgManager = roles.Any(r => r is "OrganizationOwner" or "Admin" or "SuperAdmin");

        // 1. Direct match: Actor User.Id == project.TeamLeaderId
        var isDirectTeamLead = project.TeamLeaderId.HasValue && project.TeamLeaderId.Value == actorId;

        // 2. Unassigned / External project match:
        var isUnassignedProjectTeamLead = (!project.TeamLeaderId.HasValue || project.TeamLeaderId.Value == Guid.Empty) && roles.Contains("TeamLeader");

        // 3. TeamLeader role match within the project's organization
        var isTeamLeaderInOrg = roles.Contains("TeamLeader");

        var isAuthorizedRole = isDirectTeamLead || isUnassignedProjectTeamLead || isTeamLeaderInOrg || isOrgManager;

        if (!isAuthorizedRole)
        {
            throw new UnAuthorizedTeamLeadAccessException(actorId);
        }
    }
}
