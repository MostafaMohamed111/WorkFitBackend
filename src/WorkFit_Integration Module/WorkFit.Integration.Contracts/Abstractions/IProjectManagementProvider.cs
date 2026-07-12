using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Contracts.Abstractions;

/// <summary>
/// The single interface any external PM provider must implement.
///
/// To add a new integration (e.g. GitHub, Azure DevOps):
///   1. Create a new class implementing this interface.
///   2. Register it with the DI container in your module's IRegisterModuleServices.
///   3. No other code changes are required.
/// </summary>
public interface IProjectManagementProvider
{
    /// <summary>Human-readable provider name used in logs and SyncResult (e.g. "Jira").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Loads configuration for the given organization from the database.
    /// Must be called before any Fetch* method.
    /// </summary>
    Task InitializeForOrganizationAsync(Guid organizationId, CancellationToken ct = default);

    /// <summary>
    /// Fetches all top-level projects from the external system.
    /// </summary>
    Task<IReadOnlyList<ExternalProjectDto>> FetchProjectsAsync(CancellationToken ct = default);

    /// <summary>
    /// Fetches all tasks/issues belonging to the given external project key.
    /// </summary>
    Task<IReadOnlyList<ExternalTaskDto>> FetchTasksAsync(string externalProjectKey, CancellationToken ct = default);

    /// <summary>
    /// Fetches all developer profiles (with aggregated metrics and skill signals)
    /// from the external system. Developer profiles are built by aggregating activity
    /// across all fetched issues.
    /// </summary>
    Task<IReadOnlyList<ExternalDeveloperDto>> FetchDeveloperProfilesAsync(CancellationToken ct = default);
}
