using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class CannotAttachDeletedDocumentDomainException : DomainException
{
    public CannotAttachDeletedDocumentDomainException()
        : base(
            ModuleMarker.ModuleName,
            "CANNOT_ATTACH_DELETED_DOCUMENT",
            "Cannot attach a deleted document.")
    {
    }
}