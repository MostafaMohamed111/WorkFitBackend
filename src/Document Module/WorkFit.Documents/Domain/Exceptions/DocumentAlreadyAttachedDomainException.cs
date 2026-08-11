using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentAlreadyAttachedDomainException : DomainException
{
    public DocumentAlreadyAttachedDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_ALREADY_ATTACHED",
            "Document is already attached.")
    {
    }
}