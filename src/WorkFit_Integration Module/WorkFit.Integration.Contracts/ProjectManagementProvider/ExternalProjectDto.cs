namespace WorkFit.Integration.Contracts.ProjectManagementProvider;

/// <summary>
/// Represents a project fetched from an external PM system.
/// All fields are provider-agnostic; mappers fill them.
/// </summary>
public sealed record ExternalProjectDto(
    /// <summary>The unique key in the source system (e.g. Jira project key "EC").</summary>
    string SourceKey,
    string Name,
    string? Description,
    /// <summary>Free-form status string from the provider (e.g. "active", "archived").</summary>
    string? Status,
    DateOnly? StartDate,
    DateOnly? EndDate
);

