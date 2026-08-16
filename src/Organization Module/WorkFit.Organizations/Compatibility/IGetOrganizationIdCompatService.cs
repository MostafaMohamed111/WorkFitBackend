namespace WorkFit.Organizations.Compatibility;

public interface IGetOrganizationIdCompatService
{
    Task<Guid> GetOrganizationIdAsync(Guid userId, CancellationToken ct = default);
}
