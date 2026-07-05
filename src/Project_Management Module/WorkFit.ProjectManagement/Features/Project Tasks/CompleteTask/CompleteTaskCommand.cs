using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;
public sealed record CompleteTaskCommand(Guid TaskId) : IRequest<CompleteTaskResponse>;