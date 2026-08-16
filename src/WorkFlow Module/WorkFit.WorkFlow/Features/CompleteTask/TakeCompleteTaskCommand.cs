using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.CompleteTask;

public sealed record TakeCompleteTaskCommand(Guid TaskId) : IRequest<TakeCompleteTaskResponse>;