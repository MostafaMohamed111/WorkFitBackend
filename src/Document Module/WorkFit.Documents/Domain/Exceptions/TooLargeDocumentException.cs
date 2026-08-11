using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace Expatzi.Documents.Domain.Exceptions;

public sealed class TooLargeDocumentException : DomainException
{
    public TooLargeDocumentException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_TOO_LARGE",
            "The document exceeds the maximum allowed size.")
    {
    }

    public TooLargeDocumentException(Exception innerException)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_TOO_LARGE",
            "The document exceeds the maximum allowed size.",
            inner: innerException)
    {
    }

    public TooLargeDocumentException(string message)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_TOO_LARGE",
            message)
    {
    }
}
