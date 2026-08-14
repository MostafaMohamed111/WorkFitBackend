

namespace WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

public interface ITaskLookUpService
{
    Task<TaskRecommendationContextDto> GetRecommendationContextAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);
}
