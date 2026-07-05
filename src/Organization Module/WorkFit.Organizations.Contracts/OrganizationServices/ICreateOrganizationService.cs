
namespace WorkFit.Organizations.Contracts.OrganizationServices;

public interface ICreateOrganizationService
{
    Task<Guid> CreateAsync(string organizationName, Guid userId, CancellationToken cancellationToken = default);
}
