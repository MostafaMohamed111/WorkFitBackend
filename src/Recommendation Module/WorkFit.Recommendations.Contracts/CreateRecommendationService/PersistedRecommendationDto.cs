namespace WorkFit.Recommendations.Contracts.CreateRecommendationService;

public sealed record PersistedRecommendationDto(
    Guid RecommendationId,
    Guid TaskId,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PersistedRecommendationCandidateDto> Candidates);

public sealed record PersistedRecommendationCandidateDto(
    Guid CandidateId,
    Guid EmployeeId,
    int Rank,
    decimal Score,
    IReadOnlyList<RecommendationScoreComponentDto> ScoreBreakdown,
    string Reasoning);
