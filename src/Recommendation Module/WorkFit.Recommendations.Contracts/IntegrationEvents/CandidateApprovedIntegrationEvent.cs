using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Contracts.IntegrationEvents;

public sealed record CandidateApprovedIntegrationEvent(Guid TaskId, Guid EmployeeId) : IIntegrationEvent;
