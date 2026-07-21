namespace WorkFit.Integration.Contracts.ProjectManagementProvider;

/// <summary>
/// A developer/contributor profile aggregated from external system activity.
/// </summary>
public sealed record ExternalDeveloperDto(
    /// <summary>Unique account identifier in the source system (e.g. Jira accountId).</summary>
    string SourceAccountId,
    string DisplayName,
    string Email,
    /// <summary>Job title if available from the provider; null otherwise.</summary>
    string? JobTitle,

    // ── Aggregated metrics ────────────────────────────────────────────────────
    int TotalIssues,
    int ResolvedIssues,
    int TotalStoryPoints,
    double? AvgResolutionDays,
    double BugFixRatio,
    double FeatureRatio,
    int CriticalIssuesHandled,

    /// <summary>Skill signals inferred from this developer's activity in the provider.</summary>
    IReadOnlyList<ExternalSkillSignalDto> SkillSignals
);

