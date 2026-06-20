
using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Organizations.Domain.Exceptions
{
    internal class OrganizationNameIsNullOrEmptyException : DomainException
    {
        public OrganizationNameIsNullOrEmptyException()
            : base("Organization_Name_Is_Null_Or_Empty",
                  "Organization name cannot be null or empty.",
                  "Organization name is required."
                  )
        {
        }
    }
}
