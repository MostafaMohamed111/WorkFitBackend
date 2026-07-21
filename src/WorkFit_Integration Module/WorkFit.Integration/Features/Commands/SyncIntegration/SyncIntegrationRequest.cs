namespace WorkFit.Integration.Features.Commands.SyncIntegration;

/// <summary>
/// HTTP request body for the sync endpoint.
/// Now decoupled from settings upsert to properly separate concerns.
/// </summary>
public sealed record SyncIntegrationRequest(
    Guid OrganizationId
);
