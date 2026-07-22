using WorkFit.ProjectManagement.Domain.Enums;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskType? TaskType,
    TaskPriority? Priority,
    Guid? AssigneeId,
    int? StoryPoints,
    DateOnly? DueDate,
    int AllocationPercentage
);