namespace WorkFit.Identity.Features.RegisterWithRoleForTesting;

public sealed record RegisterWithRoleForTestingRequest(
    string Email,
    string Password,
    List<string> Roles,
    string Name
    );