
using WorkFit.SharedKernel.Exceptions;

namespace WorkFit.Identity.Domain.Exceptions
{
    public sealed class UserDisplayNameIsNullOrEmptyException : DomainException
    {
        public UserDisplayNameIsNullOrEmptyException() : base(
                "USER_DISPLAY_NAME_IS_NULL_OR_EMPTY",
                "User display name cannot be null or empty.",
                "Please provide a valid display name."
            )
        {
        }
    }
}
