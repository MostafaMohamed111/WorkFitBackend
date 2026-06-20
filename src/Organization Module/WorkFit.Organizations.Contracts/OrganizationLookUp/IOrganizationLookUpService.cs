

namespace WorkFit.Organizations.Contracts.OrganizationLookUp
{
    public interface IOrganizationLookUpService
    {
        // This to be implemented by the organization module for external module communication
        Task<OrganizationLookUpDto> GetOrganizationByIdAsync(Guid organizationId);
    }
}
