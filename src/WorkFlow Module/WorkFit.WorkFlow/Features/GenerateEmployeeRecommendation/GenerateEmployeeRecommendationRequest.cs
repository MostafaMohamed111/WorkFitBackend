namespace WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

public sealed record GenerateEmployeeRecommendationRequest(
    string? Prompt,
    int? ResultLimit);
