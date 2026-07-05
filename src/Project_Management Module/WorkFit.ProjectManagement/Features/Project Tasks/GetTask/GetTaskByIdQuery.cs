using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetTask;
public sealed record GetTaskByIdQuery(Guid TaskId) : IRequest<UpdateTask.TaskDetailDto>;