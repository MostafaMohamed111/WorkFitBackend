// WorkFit.Organizations.Contracts/OrganizationMembership/IOrganizationMembershipResolver.cs
namespace WorkFit.Organizations.Contracts.OrganizationMembership;

public interface IOrganizationMembershipResolver
{
    /// Resolves the organization a user belongs to
    Task<Guid> GetOrganizationIdForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}