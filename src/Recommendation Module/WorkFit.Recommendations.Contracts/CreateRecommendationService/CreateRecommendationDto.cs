namespace WorkFit.Recommendations.Contracts.CreateRecommendationService;

public sealed record CreateRecommendationDto(
    Guid TaskId,
    IReadOnlyList<Guid> RequiredSkillIds,
    IReadOnlyList<RankedRecommendationCandidateDto> Candidates);

public sealed record RankedRecommendationCandidateDto(
    Guid EmployeeId,
    int Rank,
    decimal Score,
    IReadOnlyList<RecommendationScoreComponentDto> ScoreBreakdown,
    string Reasoning);

public sealed record RecommendationScoreComponentDto(
    string Name,
    decimal Score);
