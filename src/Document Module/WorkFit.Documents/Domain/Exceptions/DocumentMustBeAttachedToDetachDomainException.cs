using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentMustBeAttachedToDetachDomainException : DomainException
{
    public DocumentMustBeAttachedToDetachDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_MUST_BE_ATTACHED_TO_DETACH",
            "Only attached documents can be detached.")
    {
    }
}