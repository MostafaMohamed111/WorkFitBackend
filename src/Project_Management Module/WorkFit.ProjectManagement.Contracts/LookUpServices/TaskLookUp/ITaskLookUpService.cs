

namespace WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

public interface ITaskLookUpService
{
    Task<TaskDto> GetTaskByIdAsync(Guid taskId);
}
