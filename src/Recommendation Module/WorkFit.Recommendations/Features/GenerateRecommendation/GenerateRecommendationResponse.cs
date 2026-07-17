using WorkFit.Recommendations.Domain.Enums;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed record GenerateRecommendationResponse(
    Guid Id,
    Guid TaskId,
    DateTimeOffset GeneratedAt,
    int TotalCandidates,
    List<GenerateRecommendationCandidateDto> Candidates
);

public sealed record GenerateRecommendationCandidateDto(
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
