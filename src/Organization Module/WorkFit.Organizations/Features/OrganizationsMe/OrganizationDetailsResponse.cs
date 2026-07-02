namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record OrganizationDetailsResponse(
    Guid Id,
    string Name,
    Guid UserId,
    string BrandingJson,
    string SettingsJson,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
