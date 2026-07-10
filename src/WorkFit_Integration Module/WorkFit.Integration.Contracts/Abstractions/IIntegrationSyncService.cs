using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Contracts.Abstractions;

/// <summary>
/// Orchestrates a full sync from an external PM provider into the WorkFit database.
/// Inject this service wherever you need to trigger a sync (e.g. from an API endpoint
/// or a background job).
/// </summary>
public interface IIntegrationSyncService
{
    /// <summary>
    /// Runs a full sync for the given organization and department:
    ///   1. Fetches projects → persists to ProjectManagement module.
    ///   2. Fetches tasks per project → persists to ProjectManagement module.
    ///   3. Fetches developer profiles → persists employees + skill signals via module contracts.
    /// </summary>
    /// <param name="organizationId">WorkFit organization ID to associate employees with.</param>
    /// <param name="departmentId">WorkFit department ID to associate synced projects with.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SyncResult> SyncAsync(Guid organizationId, Guid departmentId, CancellationToken ct = default);
}
