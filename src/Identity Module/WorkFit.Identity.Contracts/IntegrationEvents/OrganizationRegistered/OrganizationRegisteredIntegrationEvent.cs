

using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Contracts.IntegrationEvents.OrganizationRegistered;

public sealed record OrganizationRegisteredIntegrationEvent(
        Guid UserId,
        string Email
    ) : IIntegrationEvent;

