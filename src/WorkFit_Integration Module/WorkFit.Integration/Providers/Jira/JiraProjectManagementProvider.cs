using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Contracts.Dtos;
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
    private readonly JiraSettings _settings;
    private readonly ILogger<JiraProjectManagementProvider> _logger;

    // Cache so we only hit the Jira API once per provider instance lifetime
    private IReadOnlyList<JsonElement>? _cachedIssues;

    public string ProviderName => "Jira";

    public JiraProjectManagementProvider(
        JiraApiClient client,
        IOptions<JiraSettings> settings,
        ILogger<JiraProjectManagementProvider> logger)
    {
        _client   = client;
        _settings = settings.Value;
        _logger   = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IProjectManagementProvider
    // ─────────────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<ExternalProjectDto>> FetchProjectsAsync(CancellationToken ct = default)
    {
        // Jira's issue-search API is project-scoped. We treat the configured
        // project key as a single top-level project in WorkFit.
        _logger.LogInformation("Jira: mapping project '{ProjectKey}'", _settings.ProjectKey);

        IReadOnlyList<ExternalProjectDto> result =
            [JiraIssueMapper.MapProject(_settings.ProjectKey)];

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyList<ExternalTaskDto>> FetchTasksAsync(
        string externalProjectKey, CancellationToken ct = default)
    {
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
        var issues = await GetCachedIssuesAsync(ct);
        var profiles = JiraDeveloperProfileBuilder.Build(issues);

        _logger.LogInformation(
            "Jira: built {Count} developer profiles", profiles.Count);

        return profiles;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private
    // ─────────────────────────────────────────────────────────────────────────

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
