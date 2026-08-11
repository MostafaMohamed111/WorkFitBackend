using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class CannotGrantAccessToEmptyUserDomainException : DomainException
{
    public CannotGrantAccessToEmptyUserDomainException()
        : base(
            ModuleMarker.ModuleName,
            "CANNOT_GRANT_ACCESS_TO_EMPTY_USER",
            "Cannot grant access for that user.")
    {
    }
}