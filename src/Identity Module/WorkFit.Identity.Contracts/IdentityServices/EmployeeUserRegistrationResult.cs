namespace WorkFit.Identity.Contracts.IdentityServices;

public sealed record EmployeeUserRegistrationResult(
    Guid UserId,
    string Email,
    bool IsNew,
    string? GeneratedPassword);
