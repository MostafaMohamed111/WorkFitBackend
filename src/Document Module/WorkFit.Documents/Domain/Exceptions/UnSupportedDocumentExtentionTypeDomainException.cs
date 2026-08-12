
using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class UnSupportedDocumentExtentionTypeDomainException : DomainException
{
    public UnSupportedDocumentExtentionTypeDomainException(string extension)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_EXTENSION_NOT_SUPPORTED",
            $"The document extension '{extension}' is not supported.")
    {
    }
}
