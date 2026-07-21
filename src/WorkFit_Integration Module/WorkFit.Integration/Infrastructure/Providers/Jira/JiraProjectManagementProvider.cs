using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Integration.Features.Shared;
using WorkFit.Integration.Infrastructure.Data;

namespace WorkFit.Integration.Infrastructure.Providers.Jira;

internal sealed class JiraProjectManagementProvider : IProjectManagementProvider
{
    private static readonly string[] Fields =
    [
        "summary", "description", "issuetype", "status",
        "priority", "assignee", "reporter", "labels",
        "components", "created", "resolutiondate", "updated",
        "comment", "customfield_10016", "customfield_10014", "customfield_10020"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IntegrationDbContext _integrationDb;
    private readonly ILogger<JiraProjectManagementProvider> _logger;

    private IReadOnlyList<JiraIssue>? _cachedIssues;
    private JiraSettings? _settings;

    public string ProviderName => "Jira";

    public JiraProjectManagementProvider(
        IHttpClientFactory httpClientFactory,
        IntegrationDbContext integrationDb,
        ILogger<JiraProjectManagementProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _integrationDb = integrationDb;
        _logger = logger;
    }

    public async Task InitializeForOrganizationAsync(Guid organizationId, CancellationToken ct = default)
    {
        var entity = await _integrationDb.OrganizationIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.Provider == "Jira", ct);

        if (entity is null)
            throw new EntityNotFoundException(
                moduleName: "Integration",
                objectName: "JiraSettings",
                objectId: organizationId,
                userFriendlyMessage: "No Jira settings have been configured for this organization. Please configure them before syncing.");

        _settings = new JiraSettings
        {
            BaseUrl    = entity.BaseUrl,
            Email      = entity.Email,
            ApiToken   = entity.ApiToken,
            ProjectKey = entity.ProjectKey,
            PageSize   = entity.PageSize
        };

        _logger.LogInformation(
            "Jira provider initialized for organization {OrgId}, project {ProjectKey}",
            organizationId, _settings.ProjectKey);
    }

    public Task<IReadOnlyList<ExternalProjectDto>> FetchProjectsAsync(CancellationToken ct = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Jira: mapping project '{ProjectKey}'", _settings!.ProjectKey);

        IReadOnlyList<ExternalProjectDto> result =
        [
            new ExternalProjectDto(
                SourceKey: _settings.ProjectKey,
                Name: _settings.ProjectKey.Length < 3 ? $"{_settings.ProjectKey} Project" : _settings.ProjectKey,
                Description: null,
                Status: "active",
                StartDate: null,
                EndDate: null)
        ];

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
            var issueKey = issue.Key;
            if (string.IsNullOrWhiteSpace(issueKey)) continue;

            var fields = issue.Fields;
            if (fields is null) continue;

            var projectKey = issueKey.Contains('-')
                ? issueKey[..issueKey.LastIndexOf('-')]
                : issueKey;

            if (!string.Equals(projectKey, externalProjectKey, StringComparison.OrdinalIgnoreCase))
                continue;

            var summary     = fields.Summary ?? "(no title)";
            var description = ExtractAdfText(fields.Description);
            var issueType   = fields.IssueType?.Name ?? "Task";
            var status      = fields.Status?.Name ?? "To Do";
            var priority    = fields.Priority?.Name ?? "Medium";
            var assigneeId  = fields.Assignee?.AccountId;
            var reporterId  = fields.Reporter?.AccountId;
            var storyPoints = fields.StoryPoints.HasValue ? (int)fields.StoryPoints.Value : (int?)null;
            
            DateTimeOffset? createdAt  = TryParseOffset(fields.Created);
            DateTimeOffset? resolvedAt = TryParseOffset(fields.ResolutionDate);
            DateOnly? sprintEndDate = ResolveSprintEndDate(fields.Sprints);

            DateOnly? dueDate = sprintEndDate ?? (resolvedAt.HasValue
                ? DateOnly.FromDateTime(resolvedAt.Value.UtcDateTime)
                : null);

            tasks.Add(new ExternalTaskDto(
                SourceKey:         issueKey,
                SourceProjectKey:  projectKey,
                Title:             Truncate(summary, 100)!,
                Description:       Truncate(description, 500),
                IssueType:         issueType,
                Priority:          priority,
                Status:            status,
                StoryPoints:       storyPoints,
                AssigneeAccountId: assigneeId,
                ReporterAccountId: reporterId,
                DueDate:           dueDate,
                CreatedAt:         createdAt,
                ResolvedAt:        resolvedAt));
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
        var devs = new Dictionary<string, RawDeveloperProfile>(StringComparer.Ordinal);

        foreach (var issue in issues)
        {
            var fields = issue.Fields;
            if (fields?.Assignee is null) continue;

            var devId    = fields.Assignee.AccountId;
            var devName  = fields.Assignee.DisplayName ?? "Unknown";
            var devEmail = fields.Assignee.EmailAddress ?? string.Empty;

            if (string.IsNullOrWhiteSpace(devId)) continue;

            if (!devs.TryGetValue(devId, out var dev))
            {
                dev = new RawDeveloperProfile(devId, devName, devEmail);
                devs[devId] = dev;
            }

            dev.TotalIssues++;

            var issueTypeName = fields.IssueType?.Name ?? "Unknown";
            var priorityName  = fields.Priority?.Name ?? "Unknown";
            var storyPoints   = fields.StoryPoints.HasValue ? (int)fields.StoryPoints.Value : 0;

            dev.IssueTypeBreakdown[issueTypeName] = dev.IssueTypeBreakdown.GetValueOrDefault(issueTypeName) + 1;
            dev.PriorityBreakdown[priorityName]   = dev.PriorityBreakdown.GetValueOrDefault(priorityName) + 1;
            dev.TotalStoryPoints                 += storyPoints;

            if (fields.Components is not null)
            {
                foreach (var comp in fields.Components)
                {
                    var name = comp.Name ?? "Unknown";
                    dev.Components[name] = dev.Components.GetValueOrDefault(name) + 1;
                }
            }

            if (fields.Labels is not null)
            {
                foreach (var label in fields.Labels)
                {
                    if (!string.IsNullOrWhiteSpace(label))
                        dev.Labels[label] = dev.Labels.GetValueOrDefault(label) + 1;
                }
            }

            if (!string.IsNullOrWhiteSpace(fields.ResolutionDate))
            {
                dev.ResolvedIssues++;
                if (DateTimeOffset.TryParse(fields.Created, out var c) && 
                    DateTimeOffset.TryParse(fields.ResolutionDate, out var r))
                {
                    dev.ResolutionDays.Add(Math.Round((r - c).TotalDays, 1));
                }
            }
        }

        var profiles = new List<ExternalDeveloperDto>(devs.Count);

        foreach (var dev in devs.Values)
        {
            var resDays = dev.ResolutionDays;
            double? avgRes = resDays.Count > 0 ? Math.Round(resDays.Sum() / resDays.Count, 1) : null;
            var total = Math.Max(dev.TotalIssues, 1);
            
            var bugFixRatio    = Math.Round(dev.IssueTypeBreakdown.GetValueOrDefault("Bug", 0) / (double)total, 2);
            var featureRatio   = Math.Round(dev.IssueTypeBreakdown.GetValueOrDefault("Story", 0) / (double)total, 2);
            var criticalCount  = dev.PriorityBreakdown.GetValueOrDefault("Critical", 0) + 
                                 dev.PriorityBreakdown.GetValueOrDefault("Highest",  0);

            var skillSignals = BuildSkillSignals(dev);

            profiles.Add(new ExternalDeveloperDto(
                SourceAccountId:       dev.AccountId,
                DisplayName:           dev.DisplayName,
                Email:                 dev.Email,
                JobTitle:              null,
                TotalIssues:           dev.TotalIssues,
                ResolvedIssues:        dev.ResolvedIssues,
                TotalStoryPoints:      dev.TotalStoryPoints,
                AvgResolutionDays:     avgRes,
                BugFixRatio:           bugFixRatio,
                FeatureRatio:          featureRatio,
                CriticalIssuesHandled: criticalCount,
                SkillSignals:          skillSignals));
        }

        profiles.Sort((a, b) => b.TotalStoryPoints.CompareTo(a.TotalStoryPoints));
        
        _logger.LogInformation("Jira: built {Count} developer profiles", profiles.Count);
        return profiles.AsReadOnly();
    }

    private void EnsureInitialized()
    {
        if (_settings is null)
            throw new InvalidOperationException("Call InitializeForOrganizationAsync() before invoking Fetch* methods.");
    }

    private async Task<IReadOnlyList<JiraIssue>> GetCachedIssuesAsync(CancellationToken ct)
    {
        if (_cachedIssues is not null)
            return _cachedIssues;

        var issues = new List<JiraIssue>();
        string? nextPageToken = null;

        do
        {
            var queryParams = new Dictionary<string, string>
            {
                ["jql"]        = $"project={_settings!.ProjectKey} AND assignee is not EMPTY ORDER BY updated DESC",
                ["maxResults"] = _settings.PageSize.ToString(),
                ["fields"]     = string.Join(",", Fields),
                ["expand"]     = "changelog"
            };

            if (nextPageToken is not null)
                queryParams["nextPageToken"] = nextPageToken;

            var url = BuildUrl("/rest/api/3/search/jql", queryParams);
            var responseStream = await SendStreamAsync(HttpMethod.Get, url, ct);

            var searchResponse = await JsonSerializer.DeserializeAsync<JiraSearchResponse>(
                responseStream, JsonOptions, ct);

            if (searchResponse?.Issues is not null)
                issues.AddRange(searchResponse.Issues);

            var isLast = searchResponse?.IsLast ?? true;
            nextPageToken = isLast ? null : searchResponse?.NextPageToken;
        }
        while (nextPageToken is not null);

        _cachedIssues = issues;
        return _cachedIssues;
    }

    private async Task<Stream> SendStreamAsync(HttpMethod method, string url, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("Jira");
        var request = new HttpRequestMessage(method, url);

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings!.Email}:{_settings.ApiToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(ct);
    }

    private string BuildUrl(string path, Dictionary<string, string> queryParams)
    {
        var sb = new StringBuilder(_settings!.BaseUrl.TrimEnd('/'));
        sb.Append(path).Append('?');
        foreach (var (k, v) in queryParams)
        {
            sb.Append(Uri.EscapeDataString(k)).Append('=').Append(Uri.EscapeDataString(v)).Append('&');
        }
        return sb.ToString().TrimEnd('&');
    }

    private static string? ExtractAdfText(JsonElement? descriptionElement)
    {
        if (descriptionElement is null) return null;
        var el = descriptionElement.Value;

        if (el.ValueKind == JsonValueKind.Null) return null;
        if (el.ValueKind == JsonValueKind.String) return el.GetString();
        
        if (el.ValueKind == JsonValueKind.Object)
        {
            if (el.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text" &&
                el.TryGetProperty("text", out var textEl))
                return textEl.GetString() ?? string.Empty;

            if (el.TryGetProperty("content", out var contentEl))
            {
                var parts = new List<string>();
                foreach (var child in contentEl.EnumerateArray())
                    parts.Add(ExtractAdfText(child) ?? string.Empty);
                return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            var parts = new List<string>();
            foreach (var item in el.EnumerateArray())
                parts.Add(ExtractAdfText(item) ?? string.Empty);
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        return string.Empty;
    }

    private static DateOnly? ResolveSprintEndDate(List<JiraSprint>? sprints)
    {
        if (sprints is null or { Count: 0 }) return null;
        var bestSprint = sprints.FirstOrDefault(s => s.State == "active") ?? sprints.Last();
        
        if (bestSprint.EndDate is null) return null;
        return DateTimeOffset.TryParse(bestSprint.EndDate, out var dt) ? DateOnly.FromDateTime(dt.UtcDateTime) : null;
    }

    private static IReadOnlyList<ExternalSkillSignalDto> BuildSkillSignals(RawDeveloperProfile dev)
    {
        var signals = new List<ExternalSkillSignalDto>();
        int maxComponentCount = dev.Components.Values.DefaultIfEmpty(0).Max();
        int maxLabelCount     = dev.Labels.Values.DefaultIfEmpty(0).Max();
        int totalIssues       = Math.Max(dev.TotalIssues, 1);

        foreach (var (comp, count) in dev.Components.OrderByDescending(x => x.Value).Take(10))
            signals.Add(new ExternalSkillSignalDto(comp, $"Worked on {count} issue(s) tagged with component '{comp}' in Jira.", ComputeScore(count, totalIssues, maxComponentCount), "Jira:Component"));

        foreach (var (label, count) in dev.Labels.OrderByDescending(x => x.Value).Take(10))
        {
            if (dev.Components.ContainsKey(label)) continue;
            signals.Add(new ExternalSkillSignalDto(label, $"Worked on {count} issue(s) tagged with label '{label}' in Jira.", ComputeScore(count, totalIssues, maxLabelCount), "Jira:Label"));
        }

        return signals.AsReadOnly();
    }

    private static int ComputeScore(int count, int totalIssues, int maxCount)
    {
        if (maxCount == 0) return 0;
        double rawScore = (0.6 * ((double)count / maxCount) + 0.4 * ((double)count / totalIssues)) * 100;
        return Math.Clamp((int)Math.Round(rawScore), 1, 100);
    }

    private static DateTimeOffset? TryParseOffset(string? s) => DateTimeOffset.TryParse(s, out var dt) ? dt : null;
    private static string? Truncate(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];

    // ── Internal Models ───────────────────────────────────────────────────────

    private sealed class RawDeveloperProfile(string accountId, string displayName, string email)
    {
        public string AccountId { get; } = accountId;
        public string DisplayName { get; } = displayName;
        public string Email { get; } = email;
        public int TotalIssues { get; set; }
        public int ResolvedIssues { get; set; }
        public int TotalStoryPoints { get; set; }
        public Dictionary<string, int> IssueTypeBreakdown { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int> PriorityBreakdown { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int> Components { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int> Labels { get; } = new(StringComparer.Ordinal);
        public List<double> ResolutionDays { get; } = [];
    }

    private sealed record JiraSearchResponse([property: JsonPropertyName("issues")] List<JiraIssue> Issues, [property: JsonPropertyName("isLast")] bool? IsLast, [property: JsonPropertyName("nextPageToken")] string? NextPageToken);
    private sealed record JiraIssue([property: JsonPropertyName("key")] string? Key, [property: JsonPropertyName("fields")] JiraIssueFields? Fields);
    private sealed record JiraIssueFields(
        [property: JsonPropertyName("summary")] string? Summary,
        [property: JsonPropertyName("description")] JsonElement? Description,
        [property: JsonPropertyName("issuetype")] JiraNamedField? IssueType,
        [property: JsonPropertyName("status")] JiraNamedField? Status,
        [property: JsonPropertyName("priority")] JiraNamedField? Priority,
        [property: JsonPropertyName("assignee")] JiraUser? Assignee,
        [property: JsonPropertyName("reporter")] JiraUser? Reporter,
        [property: JsonPropertyName("labels")] List<string>? Labels,
        [property: JsonPropertyName("components")] List<JiraNamedField>? Components,
        [property: JsonPropertyName("created")] string? Created,
        [property: JsonPropertyName("resolutiondate")] string? ResolutionDate,
        [property: JsonPropertyName("customfield_10016")] double? StoryPoints,
        [property: JsonPropertyName("customfield_10020")] List<JiraSprint>? Sprints);
    private sealed record JiraNamedField([property: JsonPropertyName("name")] string? Name);
    private sealed record JiraUser([property: JsonPropertyName("accountId")] string? AccountId, [property: JsonPropertyName("displayName")] string? DisplayName, [property: JsonPropertyName("emailAddress")] string? EmailAddress);
    private sealed record JiraSprint([property: JsonPropertyName("state")] string? State, [property: JsonPropertyName("endDate")] string? EndDate);
}

