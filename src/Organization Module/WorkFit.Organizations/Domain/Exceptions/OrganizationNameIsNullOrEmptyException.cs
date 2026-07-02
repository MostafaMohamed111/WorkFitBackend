using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Organizations.Domain.Exceptions;

public sealed class OrganizationNameIsNullOrEmptyException : DomainException
{
    public OrganizationNameIsNullOrEmptyException()
        : base("organization.name.empty", "Organization name cannot be empty.", "Organization name cannot be empty.")
    {
    }
}
