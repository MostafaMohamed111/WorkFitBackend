
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.TalentManagement.Domain.Exceptions;

internal sealed class CertificationWasNotFoundException : DomainException
{
    public CertificationWasNotFoundException() : base(ModuleMarker.ModuleName,
        "CERTIFICATION_NOT_FOUND",
        "The specified certification was not found.",
        "The certification you are trying to update does not exist.")
    {
        
    }

}
