namespace WorkFit.Identity.Contracts.IdentityServices;

public interface IEmployeeAccountProvisioningService
{
    Task ProvisionAsync(
        Guid userId,
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default);
}
