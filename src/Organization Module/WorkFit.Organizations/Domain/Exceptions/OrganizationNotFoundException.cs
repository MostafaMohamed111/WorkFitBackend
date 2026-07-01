using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class OrganizationNotFoundException : DomainException
{
    public OrganizationNotFoundException()
        : base("organization.not_found", "Organization was not found.", "Organization was not found.")
    {
    }
}
