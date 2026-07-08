
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.TalentManagement.Domain.Exceptions
{
    internal sealed class EmployeeSkillWasNotFoundException : DomainException
    {
        public EmployeeSkillWasNotFoundException() : base(ModuleMarker.ModuleName,
            "EMPLOYEE_SKILL_NOT_FOUND",
            "The specified employee skill was not found.",
            "The skill you are trying to update does not exist.")
        {
        }
    }
}
