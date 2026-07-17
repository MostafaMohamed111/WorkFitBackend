namespace WorkFit.Recommendations.Features.GetTaskRecommendations;

public sealed record RecommendationListItemDto(
    Guid Id,
    Guid TaskId,
    DateTimeOffset GeneratedAt,
    int TotalCandidates
);
