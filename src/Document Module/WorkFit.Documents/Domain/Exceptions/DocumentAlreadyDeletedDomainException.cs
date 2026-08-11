using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentAlreadyDeletedDomainException : DomainException
{
    public DocumentAlreadyDeletedDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_ALREADY_DELETED",
            "Document is already deleted.")
    {
    }
}