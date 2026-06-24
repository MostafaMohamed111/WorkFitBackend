
using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Organizations.Domain.Exceptions
{
    public sealed class OrganizationNameIsNullOrEmptyException : DomainException
    {
        public OrganizationNameIsNullOrEmptyException()
            : base("ORGANIZATION_NAME_IS_NULL_OR_EMPTY",
                  "Organization name cannot be null or empty.",
                  "Please provide a valid organization name."
                  )
        {
        }
    }
}
