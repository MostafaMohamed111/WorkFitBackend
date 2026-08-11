using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Contracts.IntegrationEvents;

public sealed record TaskCompletedWithCodeReviewSkillScore(string SkillKey, int? Score);

public sealed record TaskCompletedWithCodeReviewIntegrationEvent(
    Guid TaskId,
    Guid EmployeeProfileId,
    IReadOnlyList<TaskCompletedWithCodeReviewSkillScore> SkillScores) : IIntegrationEvent;
