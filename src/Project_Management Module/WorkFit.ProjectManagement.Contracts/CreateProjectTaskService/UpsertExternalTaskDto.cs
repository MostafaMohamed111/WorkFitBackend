namespace WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;

public sealed record UpsertExternalTaskDto(
    Guid ProjectId,
    string SourceSystem,
    string SourceReferenceId,
    string Title,
    string? Description,
    string TaskType,
    string Priority,
    string Status,
    Guid? AssigneeId,
    int? StoryPoints,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? UpdatedAt
);
