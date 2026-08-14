using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

public sealed record GenerateEmployeeRecommendationCommand(
    Guid TaskId,
    string? Prompt,
    int? ResultLimit) : IRequest<GenerateEmployeeRecommendationResponse>;
