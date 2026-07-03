namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record OrganizationSettingsResponse(
    Guid Id,
    string SettingsJson,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
