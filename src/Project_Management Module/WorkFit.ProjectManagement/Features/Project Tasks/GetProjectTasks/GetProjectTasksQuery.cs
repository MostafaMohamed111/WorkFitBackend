using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.MediatorContract;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetProjectTasks;

public sealed record GetProjectTasksQuery(
    Guid ProjectId,
    TaskStatus? Status,
    Guid? AssigneeId,
    TaskType? TaskType,
    TaskPriority? Priority
) : IRequest<List<TaskListItemDto>>;