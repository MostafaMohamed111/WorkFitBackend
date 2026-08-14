using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.TalentManagement.Contracts.IntegrationEvents;

public sealed record EmployeeIndexingStateChangedIntegrationEvent(
    EmployeeIndexingSnapshot Employee,
    string ChangeType,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
