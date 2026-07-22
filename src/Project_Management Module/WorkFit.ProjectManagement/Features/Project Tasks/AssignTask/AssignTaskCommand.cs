using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;
public sealed record AssignTaskCommand(Guid ProjectId, Guid TaskId, Guid AssigneeId, int? AllocationPercentage) : IRequest<Guid>;