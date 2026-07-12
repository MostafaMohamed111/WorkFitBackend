using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Contracts.Dtos;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.Integration.Providers.Jira.Mappers;

namespace WorkFit.Integration.Providers.Jira;

/// <summary>
/// Jira implementation of IProjectManagementProvider.
/// Fetches issues from Jira and maps them to the provider-agnostic integration DTOs.
///
/// To add a different PM provider (e.g. GitHub, Azure DevOps):
///   1. Create a new class implementing IProjectManagementProvider.
///   2. Register it in DI — no other changes required.
/// </summary>
public sealed class JiraProjectManagementProvider : IProjectManagementProvider
{
    private readonly JiraApiClient _client;
    private readonly IntegrationDbContext _integrationDb;
    private readonly ILogger<JiraProjectManagementProvider> _logger;

    // Cache so we only hit the Jira API once per provider instance lifetime
    private IReadOnlyList<JsonElement>? _cachedIssues;

    // Settings loaded from DB for the current organization
    private JiraSettings? _settings;

    public string ProviderName => "Jira";

    public JiraProjectManagementProvider(
        JiraApiClient client,
        IntegrationDbContext integrationDb,
        ILogger<JiraProjectManagementProvider> logger)
    {
        _client        = client;
        _integrationDb = integrationDb;
        _logger        = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IProjectManagementProvider — Initialization
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads Jira settings for the given organization from the database
    /// and configures the underlying API client.
    /// Must be called before any Fetch* method.
    /// </summary>
    public async Task InitializeForOrganizationAsync(Guid organizationId, CancellationToken ct = default)
    {
        var entity = await _integrationDb.OrganizationIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.Provider == "Jira", ct);

        if (entity is null)
            throw new InvalidOperationException(
                $"No Jira integration settings found for organization '{organizationId}'. " +
                "Please configure Jira settings via PUT /api/integration/{organizationId}/jira-settings first.");

        _settings = new JiraSettings
        {
            BaseUrl    = entity.BaseUrl,
            Email      = entity.Email,
            ApiToken   = entity.ApiToken,
            ProjectKey = entity.ProjectKey,
            PageSize   = entity.PageSize
        };

        _client.Configure(_settings);

        _logger.LogInformation(
            "Jira provider initialized for organization {OrgId}, project {ProjectKey}",
            organizationId, _settings.ProjectKey);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IProjectManagementProvider — Data fetching
    // ─────────────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<ExternalProjectDto>> FetchProjectsAsync(CancellationToken ct = default)
    {
        EnsureInitialized();

        // Jira's issue-search API is project-scoped. We treat the configured
        // project key as a single top-level project in WorkFit.
        _logger.LogInformation("Jira: mapping project '{ProjectKey}'", _settings!.ProjectKey);

        IReadOnlyList<ExternalProjectDto> result =
            [JiraIssueMapper.MapProject(_settings.ProjectKey)];

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyList<ExternalTaskDto>> FetchTasksAsync(
        string externalProjectKey, CancellationToken ct = default)
    {
        EnsureInitialized();

        var issues = await GetCachedIssuesAsync(ct);

        var tasks = new List<ExternalTaskDto>();
        foreach (var issue in issues)
        {
            var dto = JiraIssueMapper.MapTask(issue);
            if (dto is null) continue;

            // Filter to the requested project key
            if (!string.Equals(dto.SourceProjectKey, externalProjectKey, StringComparison.OrdinalIgnoreCase))
                continue;

            tasks.Add(dto);
        }

        _logger.LogInformation(
            "Jira: mapped {Count} tasks for project '{Key}'", tasks.Count, externalProjectKey);

        return tasks.AsReadOnly();
    }

    public async Task<IReadOnlyList<ExternalDeveloperDto>> FetchDeveloperProfilesAsync(
        CancellationToken ct = default)
    {
        EnsureInitialized();

        var issues = await GetCachedIssuesAsync(ct);
        var profiles = JiraDeveloperProfileBuilder.Build(issues);

        _logger.LogInformation(
            "Jira: built {Count} developer profiles", profiles.Count);

        return profiles;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private
    // ─────────────────────────────────────────────────────────────────────────

    private void EnsureInitialized()
    {
        if (_settings is null)
            throw new InvalidOperationException(
                "JiraProjectManagementProvider has not been initialized. " +
                "Call InitializeForOrganizationAsync() before invoking Fetch* methods.");
    }

    /// <summary>
    /// Fetches issues from Jira on the first call, then caches them for the
    /// lifetime of this scoped service instance (one sync operation).
    /// </summary>
    private async Task<IReadOnlyList<JsonElement>> GetCachedIssuesAsync(CancellationToken ct)
    {
        if (_cachedIssues is not null)
            return _cachedIssues;

        _cachedIssues = await _client.FetchAllIssuesAsync(ct);
        return _cachedIssues;
    }
}
