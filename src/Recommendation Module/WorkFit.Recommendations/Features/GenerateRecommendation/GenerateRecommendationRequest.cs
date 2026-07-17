namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed record GenerateRecommendationRequest(
    List<Guid> RequiredSkillIds
);
