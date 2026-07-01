namespace WorkFit.Identity.Features.RegisterOrganization;

public sealed record RegisterOrganizationRequest(string Email,
    string Password,
    string ConfirmPassword,
    string OrganizationName);