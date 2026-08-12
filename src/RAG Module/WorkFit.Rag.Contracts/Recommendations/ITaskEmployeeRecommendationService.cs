namespace WorkFit.Rag.Contracts.Recommendations;

public interface ITaskEmployeeRecommendationService
{
    Task<TaskEmployeeRecommendationResponse> RecommendAsync(
        TaskRecommendationContext context,
        CancellationToken cancellationToken = default);
}
