namespace WorkFit.WorkFlow.Features.OrganizationRegistration
{
    public sealed record OrganizationRegistrationRequest(
        string email,
        string password,
        string confirmPassword,
        string organizationName
    );
}