
using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class UnSupportedDocumentExtentionTypeException : DomainException
{
    public UnSupportedDocumentExtentionTypeException(string extension)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_EXTENSION_NOT_SUPPORTED",
            $"The document extension '{extension}' is not supported.")
    {
    }

    public UnSupportedDocumentExtentionTypeException(string extension, Exception innerException)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_EXTENSION_NOT_SUPPORTED",
            $"The document extension '{extension}' is not supported.",
            inner: innerException)
    {
    }

    public UnSupportedDocumentExtentionTypeException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_EXTENSION_NOT_SUPPORTED",
            "The document extension is not supported.")
    {
    }
}
