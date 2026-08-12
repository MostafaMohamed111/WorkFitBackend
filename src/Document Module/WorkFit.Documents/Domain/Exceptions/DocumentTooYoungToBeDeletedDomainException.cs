using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentTooYoungToBeDeletedDomainException : DomainException
{
    public DocumentTooYoungToBeDeletedDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_TOO_YOUNG_TO_BE_DELETED",
            "Only documents that have been created more than an hour ago can be deleted.")
    {
    }
}