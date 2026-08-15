namespace WorkFit.ProjectManagement.Contracts.Membership;

public interface IProjectMembershipService
{
    Task<ProjectInvitationScope?> GetInvitationScopeAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> IsTeamLeaderInOrganizationAsync(Guid teamLeaderId, Guid organizationId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(Guid projectId, Guid employeeProfileId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetMemberIdsAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public sealed record ProjectInvitationScope(Guid ProjectId, Guid OrganizationId, Guid? TeamLeaderId);
