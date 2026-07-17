using WorkFit.Recommendations.Domain.Enums;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed record GenerateRecommendationResponse(
    Guid Id,
    Guid TaskId,
    Guid GeneratedBy,
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
    DateTimeOffset? ReviewedAt,
    List<AdditionalSkillDto> AdditionalSkills
);

public sealed record AdditionalSkillDto(
    Guid SkillId,
    string SkillName,
    int ConfidenceScore
);
