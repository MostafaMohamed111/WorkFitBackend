using WorkFit.ProjectManagement.Domain.Enums;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetProjectTasks;

public sealed record GetProjectTasksRequest(
    TaskStatus? Status,
    Guid? AssigneeId,
    TaskType? TaskType,
    TaskPriority? Priority
);