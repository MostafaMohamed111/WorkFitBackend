namespace WorkFit.Integration.Contracts.Dtos;

/// <summary>
/// Represents a task/issue fetched from an external PM system.
/// </summary>
public sealed record ExternalTaskDto(
    /// <summary>Unique key from the source system (e.g. Jira issue key "EC-42").</summary>
    string SourceKey,
    /// <summary>The source project key this task belongs to (e.g. "EC").</summary>
    string SourceProjectKey,
    string Title,
    string? Description,
    /// <summary>Issue type string as returned by the provider (e.g. "Story", "Bug", "Task").</summary>
    string IssueType,
    /// <summary>Priority string as returned by the provider (e.g. "High", "Medium").</summary>
    string Priority,
    /// <summary>Status string as returned by the provider (e.g. "In Progress", "Done").</summary>
    string Status,
    int? StoryPoints,
    /// <summary>Provider-side account ID of the assignee (null if unassigned).</summary>
    string? AssigneeAccountId,
    /// <summary>Provider-side account ID of the reporter.</summary>
    string? ReporterAccountId,
    DateOnly? DueDate,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? ResolvedAt
);
