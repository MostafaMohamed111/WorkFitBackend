using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Contracts.IntegrationEvents;

public sealed record TaskAssignedIntegrationEvent(Guid TaskId, Guid EmployeeProfileId) : IIntegrationEvent; 