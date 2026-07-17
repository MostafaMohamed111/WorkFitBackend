using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetTaskRecommendations;

public sealed record GetTaskRecommendationsQuery(Guid TaskId)
    : IRequest<List<RecommendationListItemDto>>;
