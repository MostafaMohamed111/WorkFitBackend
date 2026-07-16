using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Contracts.IntegrationEvents;

public sealed record AssessmentRejectedIntegrationEvent(Guid AssessmentId) : IIntegrationEvent;
