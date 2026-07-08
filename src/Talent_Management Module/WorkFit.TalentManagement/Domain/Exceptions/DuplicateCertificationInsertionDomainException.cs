
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.TalentManagement.Domain.Exceptions
{
    public sealed class DuplicateCertificationInsertionDomainException : DomainException
    {
        public DuplicateCertificationInsertionDomainException() : base(ModuleMarker.ModuleName,
            "DUPLICATE_CERTIFICATION_INSERTION",
            "A duplicate certification insertion was attempted.",
            "You have already added this certification.")
        {
        }
    }
}
