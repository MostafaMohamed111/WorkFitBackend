
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Contracts.IntegrationEvents;

public sealed record AssessmentApprovedIntegrationEvent(Guid AssessmentId, Guid TaskId, Guid EmployeeProfileId) : IIntegrationEvent;
