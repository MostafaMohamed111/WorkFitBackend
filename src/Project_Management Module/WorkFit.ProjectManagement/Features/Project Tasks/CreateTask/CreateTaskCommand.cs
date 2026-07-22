using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskType? TaskType,
    TaskPriority? Priority,
    Guid? AssigneeId,
    int? StoryPoints,
    DateOnly? DueDate,
    int AllocationPercentage
) : IRequest<Guid>;