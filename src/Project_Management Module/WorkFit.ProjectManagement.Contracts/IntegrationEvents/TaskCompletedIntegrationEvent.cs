using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Contracts.IntegrationEvents;
public sealed record TaskCompletedIntegrationEvent(Guid TaskId, Guid EmployeeProfileId, int AllocationPercentage) : IIntegrationEvent; 