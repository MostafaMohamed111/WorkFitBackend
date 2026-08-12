namespace WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

public sealed record GenerateEmployeeRecommendationResponse(
    Guid RecommendationId,
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    Guid GeneratedBy,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<EmployeeRecommendationDto> Candidates);

public sealed record EmployeeRecommendationDto(
    Guid CandidateId,
    Guid EmployeeId,
    string EmployeeName,
    int Rank,
    decimal Score,
    double AvailableAllocation,
    IReadOnlyList<RecommendationScoreDto> ScoreBreakdown,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingSkills,
    string Reasoning);

public sealed record RecommendationScoreDto(
    string Name,
    decimal Score);
