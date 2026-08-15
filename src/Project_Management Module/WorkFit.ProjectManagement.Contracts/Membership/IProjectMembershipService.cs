namespace WorkFit.ProjectManagement.Contracts.Membership;

public interface IProjectMembershipService
{
    Task<ProjectInvitationScope?> GetInvitationScopeAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(Guid projectId, Guid employeeProfileId, Guid organizationId, CancellationToken cancellationToken = default);
}

public sealed record ProjectInvitationScope(Guid ProjectId, Guid OrganizationId, Guid? TeamLeaderId);
