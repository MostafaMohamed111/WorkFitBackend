

using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Identity.Domain.Exceptions;

internal class RoleNameIsNullOrEmptyException : DomainException
{
    public RoleNameIsNullOrEmptyException() : base(
            "ROLE_NAME_IS_NULL_OR_EMPTY",
            "Role name cannot be null or empty."
        )
    {
    }
}
