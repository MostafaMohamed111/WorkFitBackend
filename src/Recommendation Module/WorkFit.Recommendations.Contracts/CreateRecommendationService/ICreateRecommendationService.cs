namespace WorkFit.Recommendations.Contracts.CreateRecommendationService;

public interface ICreateRecommendationService
{
    Task<PersistedRecommendationDto> CreateAsync(
        CreateRecommendationDto recommendation,
        CancellationToken cancellationToken = default);
}
