using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Documents.Domain.Exceptions;

public sealed class DocumentAccessAlreadyGrantedDomainException : DomainException
{
    public DocumentAccessAlreadyGrantedDomainException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_ACCESS_ALREADY_GRANTED",
            "Access has already been granted to a user.")
    {
    }
}