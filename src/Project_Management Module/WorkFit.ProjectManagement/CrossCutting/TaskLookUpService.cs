
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class TaskLookUpService : ITaskLookUpService
{
    public Task<TaskDto> GetTaskByIdAsync(Guid taskId, CancellationToken cancellation)
    {
        // to be implemented
        throw new NotImplementedException();
    }
}
