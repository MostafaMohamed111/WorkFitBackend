namespace WorkFit.Integration.Contracts.IntegrationSyncService;

/// <summary>
/// Summary of a completed integration sync operation.
/// </summary>
public sealed record SyncResult(
    string ProviderName,
    int ProjectsSynced,
    int TasksSynced,
    int DevelopersSynced,
    int SkillSignalsSynced,
    int Errors,
    IReadOnlyList<string> ErrorMessages,
    DateTimeOffset SyncedAt,
    IReadOnlyList<UnknownDeveloperDto> UnknownDevelopers
)
{
    public bool HasErrors => Errors > 0;

    public static SyncResult Empty(string providerName) =>
        new(providerName, 0, 0, 0, 0, 0, [], DateTimeOffset.UtcNow, []);
}

public sealed record UnknownDeveloperDto(
    Guid EmployeeProfileId,
    Guid ProjectId,
    string SourceAccountId,
    string DisplayName,
    string? Email,
    int IssueCount,
    string InvitationStatus);

