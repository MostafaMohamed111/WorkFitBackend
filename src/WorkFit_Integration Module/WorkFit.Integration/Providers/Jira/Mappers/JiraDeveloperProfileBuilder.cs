using System.Text.Json;
using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Providers.Jira.Mappers;

/// <summary>
/// Aggregates Jira issues into developer profiles, replicating the logic of the
/// Python developer_data_extraction script.
///
/// Pipeline:
///   1. BuildRawProfiles(issues)  → one raw accumulator per developer
///   2. FinaliseProfiles(raw)     → compute derived stats, produce ExternalDeveloperDto list
/// </summary>
public static class JiraDeveloperProfileBuilder
{
    // ─────────────────────────────────────────────────────────────────────────
    // Entry point
    // ─────────────────────────────────────────────────────────────────────────

    public static IReadOnlyList<ExternalDeveloperDto> Build(IReadOnlyList<JsonElement> issues)
    {
        var raw = BuildRawProfiles(issues);
        return FinaliseProfiles(raw);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase 1 — accumulate per-developer data
    // ─────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, RawDeveloperProfile> BuildRawProfiles(
        IReadOnlyList<JsonElement> issues)
    {
        var devs = new Dictionary<string, RawDeveloperProfile>(StringComparer.Ordinal);

        foreach (var issue in issues)
        {
            if (!issue.TryGetProperty("fields", out var fields)) continue;
            if (!fields.TryGetProperty("assignee", out var assigneeEl)) continue;
            if (assigneeEl.ValueKind == JsonValueKind.Null) continue;

            var devId   = GetString(assigneeEl, "accountId");
            var devName = GetString(assigneeEl, "displayName") ?? "Unknown";
            var devEmail = GetString(assigneeEl, "emailAddress") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(devId)) continue;

            if (!devs.TryGetValue(devId, out var dev))
            {
                dev = new RawDeveloperProfile(devId, devName, devEmail);
                devs[devId] = dev;
            }

            dev.TotalIssues++;

            // ── issue fields ────────────────────────────────────────────────
            var itype    = GetNestedString(fields, "issuetype", "name") ?? "Unknown";
            var priority = GetNestedString(fields, "priority",  "name") ?? "Unknown";
            var resolved = GetString(fields, "resolutiondate");
            var created  = GetString(fields, "created");
            var sp       = GetStoryPoints(fields);

            dev.IssueTypeBreakdown[itype]  = dev.IssueTypeBreakdown.GetValueOrDefault(itype)  + 1;
            dev.PriorityBreakdown[priority] = dev.PriorityBreakdown.GetValueOrDefault(priority) + 1;
            dev.TotalStoryPoints           += sp;

            // ── components ──────────────────────────────────────────────────
            if (fields.TryGetProperty("components", out var compsEl))
            {
                foreach (var comp in compsEl.EnumerateArray())
                {
                    var name = GetString(comp, "name") ?? "Unknown";
                    dev.Components[name] = dev.Components.GetValueOrDefault(name) + 1;
                }
            }

            // ── labels ──────────────────────────────────────────────────────
            if (fields.TryGetProperty("labels", out var labelsEl))
            {
                foreach (var label in labelsEl.EnumerateArray())
                {
                    var lbl = label.GetString();
                    if (!string.IsNullOrWhiteSpace(lbl))
                        dev.Labels[lbl] = dev.Labels.GetValueOrDefault(lbl) + 1;
                }
            }

            // ── resolution ──────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                dev.ResolvedIssues++;
                var resDay = DaysToResolve(created, resolved);
                if (resDay.HasValue)
                    dev.ResolutionDays.Add(resDay.Value);
            }

            // ── comments authored by this developer ─────────────────────────
            if (fields.TryGetProperty("comment", out var commentSection) &&
                commentSection.TryGetProperty("comments", out var comments))
            {
                foreach (var c in comments.EnumerateArray())
                {
                    var author = c.TryGetProperty("author", out var a) ? a : default;
                    if (GetString(author, "accountId") == devId)
                        dev.TotalCommentsMade++;
                }
            }

            // ── cycle times from changelog ──────────────────────────────────
            if (issue.TryGetProperty("changelog", out var changelog))
            {
                var ct = JiraApiClient.ExtractCycleTimes(changelog);
                foreach (var (status, days) in ct)
                    dev.CycleTimes[status] = dev.CycleTimes.GetValueOrDefault(status) + days;
            }
        }

        return devs;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase 2 — finalise and produce ExternalDeveloperDto list
    // ─────────────────────────────────────────────────────────────────────────

    private static IReadOnlyList<ExternalDeveloperDto> FinaliseProfiles(
        Dictionary<string, RawDeveloperProfile> devs)
    {
        var profiles = new List<ExternalDeveloperDto>(devs.Count);

        foreach (var dev in devs.Values)
        {
            var resDays = dev.ResolutionDays;
            double? avgRes = resDays.Count > 0
                ? Math.Round(resDays.Sum() / resDays.Count, 1)
                : null;

            var total = Math.Max(dev.TotalIssues, 1);
            var bugFixRatio    = Math.Round(dev.IssueTypeBreakdown.GetValueOrDefault("Bug", 0)   / (double)total, 2);
            var featureRatio   = Math.Round(dev.IssueTypeBreakdown.GetValueOrDefault("Story", 0) / (double)total, 2);
            var criticalCount  = dev.PriorityBreakdown.GetValueOrDefault("Critical", 0)
                               + dev.PriorityBreakdown.GetValueOrDefault("Highest",  0);

            var skillSignals = BuildSkillSignals(dev);

            profiles.Add(new ExternalDeveloperDto(
                SourceAccountId:       dev.AccountId,
                DisplayName:           dev.DisplayName,
                Email:                 dev.Email,
                JobTitle:              null,   // Jira doesn't expose job title in issue fields
                TotalIssues:           dev.TotalIssues,
                ResolvedIssues:        dev.ResolvedIssues,
                TotalStoryPoints:      dev.TotalStoryPoints,
                AvgResolutionDays:     avgRes,
                BugFixRatio:           bugFixRatio,
                FeatureRatio:          featureRatio,
                CriticalIssuesHandled: criticalCount,
                SkillSignals:          skillSignals));
        }

        // Sort by story points descending (mirrors Python script)
        profiles.Sort((a, b) => b.TotalStoryPoints.CompareTo(a.TotalStoryPoints));
        return profiles.AsReadOnly();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Skill signal generation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts component/label usage counts into ExternalSkillSignalDto list.
    /// Score = proportional to usage count, normalized 0–100.
    /// </summary>
    private static IReadOnlyList<ExternalSkillSignalDto> BuildSkillSignals(
        RawDeveloperProfile dev)
    {
        var signals = new List<ExternalSkillSignalDto>();

        int maxComponentCount = dev.Components.Values.DefaultIfEmpty(0).Max();
        int maxLabelCount     = dev.Labels.Values.DefaultIfEmpty(0).Max();
        int totalIssues       = Math.Max(dev.TotalIssues, 1);

        // Components → skill signals
        foreach (var (component, count) in dev.Components.OrderByDescending(x => x.Value).Take(10))
        {
            int score = ComputeScore(count, totalIssues, maxComponentCount);
            signals.Add(new ExternalSkillSignalDto(
                SkillName:      component,
                Evidence:       $"Worked on {count} issue(s) tagged with component '{component}' in Jira.",
                ConfidenceScore: score,
                SourceLabel:    "Jira:Component"));
        }

        // Labels → skill signals
        foreach (var (label, count) in dev.Labels.OrderByDescending(x => x.Value).Take(10))
        {
            // Skip if a component already covers the same name
            if (dev.Components.ContainsKey(label)) continue;

            int score = ComputeScore(count, totalIssues, maxLabelCount);
            signals.Add(new ExternalSkillSignalDto(
                SkillName:      label,
                Evidence:       $"Worked on {count} issue(s) tagged with label '{label}' in Jira.",
                ConfidenceScore: score,
                SourceLabel:    "Jira:Label"));
        }

        return signals.AsReadOnly();
    }

    /// <summary>
    /// Normalizes a usage count to a 0–100 confidence score.
    /// Uses a blend of relative-to-max and relative-to-total to give a fair score
    /// even when a developer worked across many different areas.
    /// </summary>
    private static int ComputeScore(int count, int totalIssues, int maxCount)
    {
        if (maxCount == 0) return 0;

        // Relative to max usage by this developer (0–1)
        double relativeToMax   = (double)count / maxCount;
        // Relative to total issues (density 0–1)
        double relativeToTotal = (double)count / totalIssues;

        // Weighted blend: 60% max-relative, 40% density
        double rawScore = (0.6 * relativeToMax + 0.4 * relativeToTotal) * 100;
        return Math.Clamp((int)Math.Round(rawScore), 1, 100);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static double? DaysToResolve(string? createdStr, string? resolvedStr)
    {
        if (!DateTimeOffset.TryParse(createdStr,  out var c)) return null;
        if (!DateTimeOffset.TryParse(resolvedStr, out var r)) return null;
        return Math.Round((r - c).TotalDays, 1);
    }

    private static int GetStoryPoints(JsonElement fields)
    {
        if (!fields.TryGetProperty("customfield_10016", out var sp)) return 0;
        if (sp.ValueKind == JsonValueKind.Null) return 0;
        return sp.ValueKind == JsonValueKind.Number ? (int)sp.GetDouble() : 0;
    }

    private static string? GetString(JsonElement el, string prop) =>
        el.ValueKind != JsonValueKind.Null &&
        el.TryGetProperty(prop, out var v) &&
        v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static string? GetNestedString(JsonElement el, string prop, string nested)
    {
        if (!el.TryGetProperty(prop, out var outer)) return null;
        if (outer.ValueKind == JsonValueKind.Null) return null;
        return GetString(outer, nested);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Accumulator (private to this class)
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class RawDeveloperProfile(string accountId, string displayName, string email)
    {
        public string AccountId   { get; } = accountId;
        public string DisplayName { get; } = displayName;
        public string Email       { get; } = email;

        public int TotalIssues        { get; set; }
        public int ResolvedIssues     { get; set; }
        public int TotalStoryPoints   { get; set; }
        public int TotalCommentsMade  { get; set; }

        public Dictionary<string, int>    IssueTypeBreakdown { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int>    PriorityBreakdown  { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int>    Components         { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, int>    Labels             { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, double> CycleTimes         { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<double>               ResolutionDays     { get; } = [];
    }
}
