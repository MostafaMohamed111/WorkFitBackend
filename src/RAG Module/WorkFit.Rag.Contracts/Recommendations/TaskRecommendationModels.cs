namespace WorkFit.Rag.Contracts.Recommendations;

public sealed record TaskRecommendationContext(
    Guid TaskId,
    string TaskTitle,
    string? TaskDescription,
    Guid ProjectId,
    string ProjectName,
    string? ProjectDescription,
    Guid OrganizationId,
    string? OrganizationName,
    string? Prompt,
    double RequestedAllocation,
    IReadOnlyList<RequiredSkill> RequiredSkills,
    int ResultLimit = 10,
    string RequiredEmployeeStatus = "Active");

public sealed record RequiredSkill(
    Guid? SkillId,
    string Name,
    double RequiredLevel,
    double Weight = 1);

public sealed record TaskEmployeeRecommendationResponse(
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    IReadOnlyList<RankedEmployeeRecommendation> Employees);

public sealed record RankedEmployeeRecommendation(
    int Rank,
    Guid EmployeeProfileId,
    string EmployeeName,
    double AvailableAllocation,
    double SemanticScore,
    double SkillScore,
    double PerformanceScore,
    double LlmEfficiencyScore,
    double FinalScore,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingSkills,
    string GroundedReasoning);
