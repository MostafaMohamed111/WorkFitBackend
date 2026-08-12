namespace WorkFit.Organizations.Contracts.OrganizationServices;

public interface IGetOrganizationIdService
{
    Task<Guid> GetOrganizationIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
