namespace WorkFit.TalentManagement.Contracts.Invitations;

public interface IDeveloperInvitationTalentService
{
    Task<PendingDeveloperDto?> GetPendingDeveloperAsync(
        Guid organizationId,
        Guid employeeProfileId,
        string sourceSystem,
        string sourceAccountId,
        CancellationToken cancellationToken = default);

    Task LinkAndActivateAsync(
        Guid organizationId,
        Guid employeeProfileId,
        Guid userId,
        string displayName,
        string email,
        CancellationToken cancellationToken = default);
}

public sealed record PendingDeveloperDto(Guid EmployeeProfileId, string DisplayName, string? Email);
