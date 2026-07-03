using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;
public sealed record AssignTaskCommand(Guid TaskId, Guid AssigneeId) : IRequest<AssignTaskResponse>;