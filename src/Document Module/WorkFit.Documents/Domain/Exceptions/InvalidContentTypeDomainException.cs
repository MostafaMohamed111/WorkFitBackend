using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class InvalidContentTypeDomainException : DomainException
{
    public InvalidContentTypeDomainException(string contentType)
        : base(
            ModuleMarker.ModuleName,
            "INVALID_CONTENT_TYPE",
            $"Content type '{contentType}' is not allowed.")
    {
    }
}