using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetRecommendationById;

public sealed record GetRecommendationByIdQuery(Guid Id) : IRequest<RecommendationDetailDto>;
