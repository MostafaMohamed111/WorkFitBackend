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
    DateTimeOffset SyncedAt
)
{
    public bool HasErrors => Errors > 0;

    public static SyncResult Empty(string providerName) =>
        new(providerName, 0, 0, 0, 0, 0, [], DateTimeOffset.UtcNow);
}

