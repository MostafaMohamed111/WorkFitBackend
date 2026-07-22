using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskAllocation;

public sealed record SetTaskAllocationCommand(Guid TaskId, int AllocationPercentage) : IRequest<Guid>;