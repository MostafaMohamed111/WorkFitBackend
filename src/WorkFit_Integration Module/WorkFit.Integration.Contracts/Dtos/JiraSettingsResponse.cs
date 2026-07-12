namespace WorkFit.Integration.Contracts.Dtos;

/// <summary>
/// Response DTO for Jira integration settings.
/// ApiToken is masked for security — only the last 4 characters are visible.
/// </summary>
public sealed record JiraSettingsResponse(
    Guid Id,
    Guid OrganizationId,
    string BaseUrl,
    string Email,
    string MaskedApiToken,
    string ProjectKey,
    int PageSize,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
