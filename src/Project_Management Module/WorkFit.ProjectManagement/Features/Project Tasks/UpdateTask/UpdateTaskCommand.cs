using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.MediatorContract;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string? Title,
    string? Description,
    TaskStatus? Status,
    TaskPriority? Priority,
    DateOnly? DueDate,
    int? StoryPoints
) : IRequest<TaskDetailDto>;