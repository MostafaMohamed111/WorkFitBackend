namespace WorkFit.Integration.Contracts.ProjectManagementProvider;

/// <summary>
/// A single skill signal inferred from external system activity.
/// E.g. "developer worked on 12 issues tagged with the 'Payment' component".
/// </summary>
public sealed record ExternalSkillSignalDto(
    /// <summary>Human-readable skill / competency name (e.g. "Payment", "Backend", "Docker").</summary>
    string SkillName,
    /// <summary>A human-readable description of the evidence (used for confidence audit trail).</summary>
    string Evidence,
    /// <summary>Confidence score in the range 0–100.</summary>
    int ConfidenceScore,
    /// <summary>Label identifying the source (e.g. "Jira:Component", "Jira:Label").</summary>
    string SourceLabel
);

