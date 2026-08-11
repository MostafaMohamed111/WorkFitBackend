using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class InvalidSizeDomainException : DomainException
{
    public InvalidSizeDomainException()
        : base(
            ModuleMarker.ModuleName,
            "INVALID_SIZE",
            "Size must be greater than zero.")
    {
    }
}