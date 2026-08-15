namespace WorkFit.Identity.Contracts.IdentityServices;

public interface ICreateEmployeeUserService
{
    Task<EmployeeUserRegistrationResult> GetOrCreateAsync(
        string email,
        string displayName,
    string password,
        CancellationToken cancellationToken = default);
}
