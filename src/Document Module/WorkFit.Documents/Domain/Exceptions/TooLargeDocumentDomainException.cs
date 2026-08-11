using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class TooLargeDocumentDomainException : DomainException
{
    public TooLargeDocumentDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_TOO_LARGE",
            "The document exceeds the maximum allowed size.")
    {
    }
}
