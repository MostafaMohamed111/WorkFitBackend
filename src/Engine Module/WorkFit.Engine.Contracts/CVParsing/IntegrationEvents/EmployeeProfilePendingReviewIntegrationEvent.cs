using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Contracts.CVParsing.IntegrationEvents;

public sealed record EmployeeProfilePendingReviewIntegrationEvent(
    Guid EmployeeProfileId,
    Guid OrganizationId,
    Guid CVParseJobId,
    string? ParsedName,
    string? ParsedEmail) : IIntegrationEvent;
