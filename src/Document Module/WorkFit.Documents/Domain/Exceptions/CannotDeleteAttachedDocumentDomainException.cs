using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class CannotDeleteAttachedDocumentDomainException : DomainException
{
    public CannotDeleteAttachedDocumentDomainException()
        : base(
            ModuleMarker.ModuleName,
            "CANNOT_DELETE_ATTACHED_DOCUMENT",
            "Cannot delete an attached document. Detach it first.")
    {
    }
}