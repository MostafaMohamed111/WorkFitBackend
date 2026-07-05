
namespace WorkFit.Identity.Contracts.IdentityServices;

public interface ICreateOrganizationUserService
{
    Task<Guid> RegisterAsync(string email, string password, string confirmPassword, string organizationName, CancellationToken cancellationToken = default);
}
