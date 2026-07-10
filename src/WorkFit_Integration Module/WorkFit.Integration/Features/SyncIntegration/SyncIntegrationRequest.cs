using FastEndpoints;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Features.SyncIntegration;

/// <summary>
/// HTTP request body for the sync endpoint.
/// </summary>
public sealed record SyncIntegrationRequest(
    Guid OrganizationId,
    Guid DepartmentId
);
