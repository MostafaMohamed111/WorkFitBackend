using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.DeleteTask;

public sealed record DeleteTaskCommand(Guid TaskId) : IRequest;