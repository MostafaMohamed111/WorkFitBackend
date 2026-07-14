namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskAllocation;
public sealed record SetTaskAllocationResponse(Guid Id, int AllocationPercentage, bool IsActive);