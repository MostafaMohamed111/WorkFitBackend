
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Contracts.Events;

public record EmployeeRegisteredIntegrationEvent(
        string email,
        string password
    ) : IIntegrationEvent;
