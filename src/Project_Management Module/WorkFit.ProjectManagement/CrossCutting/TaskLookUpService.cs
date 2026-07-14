
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class TaskLookUpService : ITaskLookUpService
{
    public Task<TaskDto> GetTaskByIdAsync(Guid taskId)
    {
        // to be implemented
        throw new NotImplementedException();
    }
}
