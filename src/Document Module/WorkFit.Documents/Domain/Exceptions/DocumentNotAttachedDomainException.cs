using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentNotAttachedDomainException : DomainException
{
    public DocumentNotAttachedDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_NOT_ATTACHED",
            "The document is not attached to any entity and cannot be accessed.")
    {
    }
}