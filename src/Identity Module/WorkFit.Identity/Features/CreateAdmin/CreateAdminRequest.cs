namespace WorkFit.Identity.Features.CreateAdmin;

public sealed record CreateAdminRequest(
    string Email,
    string Password,
    string Name);
