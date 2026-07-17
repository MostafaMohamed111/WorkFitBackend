using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed record GenerateRecommendationCommand(
    Guid TaskId,
    List<Guid> RequiredSkillIds
) : IRequest<GenerateRecommendationResponse>;
