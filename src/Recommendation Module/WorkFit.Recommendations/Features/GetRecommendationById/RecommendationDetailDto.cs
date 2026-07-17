using WorkFit.Recommendations.Domain.Enums;

namespace WorkFit.Recommendations.Features.GetRecommendationById;

public sealed record RecommendationDetailDto(
    Guid Id,
    Guid TaskId,
    DateTimeOffset GeneratedAt,
    string RequiredSkillsSnapshot,
    List<CandidateDetailDto> Candidates
);

public sealed record CandidateDetailDto(
    Guid EmployeeId,
    decimal MatchScore,
    string MatchReasoning,
    int Rank,
    CandidateStatus Status,
    Guid? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    Guid? RejectedBy,
    DateTimeOffset? RejectedAt
);
