namespace WorkFit.Recommendations.Features.GetTaskRecommendations;

public sealed record RecommendationListItemDto(
    Guid Id,
    Guid TaskId,
    Guid GeneratedBy,
    DateTimeOffset GeneratedAt,
    int TotalCandidates
);
