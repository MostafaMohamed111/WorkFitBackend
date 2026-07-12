namespace WorkFit.Integration.Features.SyncIntegration;

/// <summary>
/// HTTP request body for the sync endpoint.
/// Includes Jira settings so the organization's configuration is saved
/// before triggering the sync — no separate PUT required.
/// </summary>
public sealed record SyncIntegrationRequest(
    Guid OrganizationId,
    Guid DepartmentId,
    string BaseUrl,
    string Email,
    string ApiToken,
    string ProjectKey,
    int PageSize = 100
);
