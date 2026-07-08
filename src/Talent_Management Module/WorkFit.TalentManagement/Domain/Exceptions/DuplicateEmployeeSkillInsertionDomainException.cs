using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.TalentManagement.Domain.Exceptions;

internal sealed class DuplicateEmployeeSkillInsertionDomainException : DomainException
{
    public DuplicateEmployeeSkillInsertionDomainException() : base(ModuleMarker.ModuleName,
        "DUPLICATE_EMPLOYEE_SKILL_INSERTION",
        "A duplicate skill insertion was attempted.",
        "You have already added this skill.")
    {
    }
}