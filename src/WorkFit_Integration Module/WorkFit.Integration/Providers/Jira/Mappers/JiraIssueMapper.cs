using System.Text.Json;
using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Providers.Jira.Mappers;

/// <summary>
/// Maps raw Jira issue JSON elements to WorkFit integration DTOs.
/// All mapping logic is isolated here — no Jira specifics leak into other layers.
/// </summary>
public static class JiraIssueMapper
{
    // ─────────────────────────────────────────────────────────────────────────
    // Project mapping
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Derives a single ExternalProjectDto from the configured Jira project key.
    /// Jira's /search endpoint doesn't return top-level project metadata directly,
    /// so we infer it from the first issue in the batch.
    /// </summary>
    public static ExternalProjectDto MapProject(string projectKey)
    {
        return new ExternalProjectDto(
            SourceKey: projectKey,
            Name: projectKey.Length < 3 ? $"{projectKey} Project" : projectKey,          // Callers may override with a richer name from the Jira project API
            Description: null,
            Status: "active",
            StartDate: null,
            EndDate: null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Task mapping
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps a raw Jira issue JSON element to an ExternalTaskDto.
    /// Returns null when the issue cannot be reliably mapped (e.g. missing key).
    /// </summary>
    public static ExternalTaskDto? MapTask(JsonElement issue)
    {
        if (!issue.TryGetProperty("key", out var keyEl)) return null;
        var issueKey = keyEl.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(issueKey)) return null;

        var fields = issue.GetProperty("fields");

        // ── Project key (e.g. "EC" from "EC-42") ─────────────────────────────
        var projectKey = issueKey.Contains('-') ? issueKey[..issueKey.LastIndexOf('-')] : issueKey;

        // ── Scalar fields ─────────────────────────────────────────────────────
        var summary     = GetString(fields, "summary") ?? "(no title)";
        var description = ExtractDescription(fields);
        var issueType   = GetNestedString(fields, "issuetype", "name") ?? "Task";
        var status      = GetNestedString(fields, "status",    "name") ?? "To Do";
        var priority    = GetNestedString(fields, "priority",  "name") ?? "Medium";
        var assigneeId  = GetNestedString(fields, "assignee",  "accountId");
        var reporterId  = GetNestedString(fields, "reporter",  "accountId");
        var createdStr  = GetString(fields, "created");
        var resolvedStr = GetString(fields, "resolutiondate");

        // ── Story points (customfield_10016) ──────────────────────────────────
        int? storyPoints = null;
        if (fields.TryGetProperty("customfield_10016", out var spEl) &&
            spEl.ValueKind != JsonValueKind.Null)
        {
            storyPoints = spEl.ValueKind == JsonValueKind.Number
                ? (int?)spEl.GetDouble()
                : null;
        }

        // ── Dates ─────────────────────────────────────────────────────────────
        DateTimeOffset? createdAt  = TryParseOffset(createdStr);
        DateTimeOffset? resolvedAt = TryParseOffset(resolvedStr);

        // ── Sprint End Date (customfield_10020) ───────────────────────────────
        DateOnly? sprintEndDate = null;
        if (fields.TryGetProperty("customfield_10020", out var sprintArray) && sprintArray.ValueKind == JsonValueKind.Array)
        {
            // Jira can return multiple sprints if an issue rolled over.
            // We prioritize the "active" sprint, falling back to the last one.
            JsonElement? bestSprint = null;

            foreach (var sprintObj in sprintArray.EnumerateArray())
            {
                bestSprint = sprintObj; // Keep tracking the last one
                if (sprintObj.TryGetProperty("state", out var stateEl) && stateEl.GetString() == "active")
                {
                    break; // Found the active sprint, stop looking
                }
            }

            if (bestSprint.HasValue && bestSprint.Value.TryGetProperty("endDate", out var endDateEl) && endDateEl.ValueKind == JsonValueKind.String)
            {
                if (DateTimeOffset.TryParse(endDateEl.GetString(), out var sprintDt))
                {
                    sprintEndDate = DateOnly.FromDateTime(sprintDt.UtcDateTime);
                }
            }
        }

        // For DueDate: use sprint end date if available, otherwise fallback to resolution date
        DateOnly? dueDate = sprintEndDate ?? (resolvedAt.HasValue
            ? DateOnly.FromDateTime(resolvedAt.Value.UtcDateTime)
            : null);

        return new ExternalTaskDto(
            SourceKey:        issueKey,
            SourceProjectKey: projectKey,
            Title:            Truncate(summary, 100),
            Description:      Truncate(description, 500),
            IssueType:        issueType,
            Priority:         priority,
            Status:           status,
            StoryPoints:      storyPoints,
            AssigneeAccountId: assigneeId,
            ReporterAccountId: reporterId,
            DueDate:          dueDate,
            CreatedAt:        createdAt,
            ResolvedAt:       resolvedAt);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Enum mapping helpers (Jira strings → WorkFit enum names)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Maps a Jira status string to a WorkFit TaskStatus name.</summary>
    public static string MapTaskStatus(string jiraStatus) =>
        jiraStatus.Trim().ToLowerInvariant() switch
        {
            "to do"         => "ToDo",
            "open"          => "ToDo",
            "backlog"       => "ToDo",
            "in progress"   => "InProgress",
            "in review"     => "Review",
            "review"        => "Review",
            "code review"   => "Review",
            "done"          => "Done",
            "closed"        => "Done",
            "resolved"      => "Done",
            _               => "ToDo"
        };

    /// <summary>Maps a Jira priority string to a WorkFit TaskPriority name.</summary>
    public static string MapTaskPriority(string jiraPriority) =>
        jiraPriority.Trim().ToLowerInvariant() switch
        {
            "lowest"   => "Low",
            "low"      => "Low",
            "medium"   => "Medium",
            "high"     => "High",
            "highest"  => "Critical",
            "critical" => "Critical",
            _          => "Medium"
        };

    /// <summary>Maps a Jira issue type string to a WorkFit TaskType name.</summary>
    public static string MapTaskType(string jiraType) =>
        jiraType.Trim().ToLowerInvariant() switch
        {
            "story"    => "Story",
            "bug"      => "Bug",
            "epic"     => "Epic",
            "task"     => "Task",
            "sub-task" => "SubTask",
            "subtask"  => "SubTask",
            _          => "Task"
        };

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string? ExtractDescription(JsonElement fields)
    {
        if (!fields.TryGetProperty("description", out var descEl)) return null;
        if (descEl.ValueKind == JsonValueKind.Null) return null;
        if (descEl.ValueKind == JsonValueKind.String) return descEl.GetString();
        if (descEl.ValueKind == JsonValueKind.Object)
            return JiraApiClient.ExtractAdfText(descEl);
        return null;
    }

    private static string? GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static string? GetNestedString(JsonElement el, string prop, string nested)
    {
        if (!el.TryGetProperty(prop, out var outer)) return null;
        if (outer.ValueKind == JsonValueKind.Null) return null;
        return GetString(outer, nested);
    }

    private static DateTimeOffset? TryParseOffset(string? s) =>
        DateTimeOffset.TryParse(s, out var dt) ? dt : null;

    private static string? Truncate(string? s, int max) =>
        s is null ? null : s.Length <= max ? s : s[..max];
}
