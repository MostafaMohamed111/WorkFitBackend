using WorkFit.ProjectManagement.Domain.Enums;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;

public sealed record TaskDetailDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskType TaskType,
    TaskStatus Status,
    TaskPriority Priority,
    Guid? AssigneeId,
    Guid CreatedById,
    int? StoryPoints,
    DateOnly? DueDate,
    string? SourceSystem,
    string? SourceReferenceId,
    DateTimeOffset? CompletedAt,
    DateTime CreatedAt
);