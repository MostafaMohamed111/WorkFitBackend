using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WorkFit.Integration.Providers.Jira;

/// <summary>
/// Low-level Jira REST API v3 client.
/// Handles HTTP, Basic Auth, pagination, and ADF→plain-text conversion.
/// </summary>
public sealed class JiraApiClient
{
    private static readonly string[] Fields =
    [
        "summary", "description", "issuetype", "status",
        "priority", "assignee", "reporter", "labels",
        "components", "created", "resolutiondate", "updated",
        "comment",
        "customfield_10016", // story points
        "customfield_10014", // epic link
        "customfield_10020"  // sprint data
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JiraSettings _settings;
    private readonly ILogger<JiraApiClient> _logger;

    public JiraApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<JiraSettings> settings,
        ILogger<JiraApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches all issues from the configured Jira project that have an assignee,
    /// including their full changelog (needed for cycle-time extraction).
    /// </summary>
    public async Task<IReadOnlyList<JsonElement>> FetchAllIssuesAsync(CancellationToken ct = default)
    {
        var issues = new List<JsonElement>();
        string? nextPageToken = null;

        _logger.LogInformation("Jira: starting issue fetch for project {ProjectKey}", _settings.ProjectKey);

        do
        {
            var queryParams = new Dictionary<string, string>
            {
                ["jql"]        = $"project={_settings.ProjectKey} AND assignee is not EMPTY ORDER BY updated DESC",
                ["maxResults"] = _settings.PageSize.ToString(),
                ["fields"]     = string.Join(",", Fields),
                ["expand"]     = "changelog"
            };

            if (nextPageToken is not null)
                queryParams["nextPageToken"] = nextPageToken;

            var url = BuildUrl("/rest/api/3/search/jql", queryParams);
            var response = await SendAsync(HttpMethod.Get, url, ct);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            foreach (var issue in root.GetProperty("issues").EnumerateArray())
            {
                // Clone each element so it outlives the JsonDocument
                issues.Add(issue.Clone());
            }

            _logger.LogInformation("Jira: fetched {Total} issues so far...", issues.Count);

            bool isLast = !root.TryGetProperty("isLast", out var isLastProp) || isLastProp.GetBoolean();
            nextPageToken = isLast ? null
                : root.TryGetProperty("nextPageToken", out var npt) ? npt.GetString() : null;
        }
        while (nextPageToken is not null);

        _logger.LogInformation("Jira: finished fetching – {Total} total issues", issues.Count);
        return issues;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Static helpers (no network)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Recursively flattens Atlassian Document Format (ADF) JSON to plain text.
    /// Port of the Python extract_adf_text() function.
    /// </summary>
    public static string ExtractAdfText(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.String)
            return node.GetString() ?? string.Empty;

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("type", out var typeEl) &&
                typeEl.GetString() == "text" &&
                node.TryGetProperty("text", out var textEl))
            {
                return textEl.GetString() ?? string.Empty;
            }

            if (node.TryGetProperty("content", out var contentEl))
            {
                var parts = new List<string>();
                foreach (var child in contentEl.EnumerateArray())
                    parts.Add(ExtractAdfText(child));
                return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            return string.Empty;
        }

        if (node.ValueKind == JsonValueKind.Array)
        {
            var parts = new List<string>();
            foreach (var item in node.EnumerateArray())
                parts.Add(ExtractAdfText(item));
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts time spent in each Jira status (in days) from the issue changelog.
    /// Port of the Python extract_cycle_times() function.
    /// </summary>
    public static Dictionary<string, double> ExtractCycleTimes(JsonElement changelog)
    {
        var transitions = new List<(string To, DateTimeOffset At)>();

        if (!changelog.TryGetProperty("histories", out var histories))
            return [];

        foreach (var history in histories.EnumerateArray())
        {
            if (!history.TryGetProperty("created", out var createdEl)) continue;
            if (!DateTimeOffset.TryParse(createdEl.GetString(), out var createdAt)) continue;

            if (!history.TryGetProperty("items", out var items)) continue;
            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("field", out var fieldEl) &&
                    fieldEl.GetString() == "status" &&
                    item.TryGetProperty("toString", out var toEl))
                {
                    transitions.Add((toEl.GetString() ?? "Unknown", createdAt));
                }
            }
        }

        transitions.Sort((a, b) => a.At.CompareTo(b.At));

        var cycleTimes = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < transitions.Count; i++)
        {
            var (status, start) = transitions[i];
            var end = i + 1 < transitions.Count
                ? transitions[i + 1].At
                : DateTimeOffset.UtcNow;

            var days = (end - start).TotalDays;
            cycleTimes[status] = cycleTimes.TryGetValue(status, out var existing)
                ? Math.Round(existing + days, 2)
                : Math.Round(days, 2);
        }

        return cycleTimes;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<string> SendAsync(HttpMethod method, string url, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient("Jira");
        using var request = new HttpRequestMessage(method, url);

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.Email}:{_settings.ApiToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    private string BuildUrl(string path, Dictionary<string, string> queryParams)
    {
        var sb = new StringBuilder(_settings.BaseUrl.TrimEnd('/'));
        sb.Append(path);
        sb.Append('?');
        foreach (var (k, v) in queryParams)
        {
            sb.Append(Uri.EscapeDataString(k));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(v));
            sb.Append('&');
        }
        return sb.ToString().TrimEnd('&');
    }
}
