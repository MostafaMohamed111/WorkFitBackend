using WorkFit.ProjectManagement.Domain.Enums;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetProjectTasks;

public sealed record TaskListItemDto(
    Guid Id,
    string Title,
    TaskType TaskType,
    TaskStatus Status,
    TaskPriority Priority,
    Guid? AssigneeId,
    int? StoryPoints,
    DateOnly? DueDate,
    DateTimeOffset? CompletedAt
);