using WorkFit.ProjectManagement.Domain.Enums;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;
public sealed record UpdateTaskRequest(
    string? Title,
    string? Description,
    TaskStatus? Status,
    TaskPriority? Priority,
    DateOnly? DueDate,
    int? StoryPoints
);