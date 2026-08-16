namespace WorkFit.ProjectManagement.Contracts.CompleteTaskService;

public interface ICompleteProjectTaskService
{
    Task<TaskCompletionContextDto> CompleteTaskAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);
}