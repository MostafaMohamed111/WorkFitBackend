namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;
public sealed record AssignTaskRequest(Guid AssigneeId, int? AllocationPercentage);