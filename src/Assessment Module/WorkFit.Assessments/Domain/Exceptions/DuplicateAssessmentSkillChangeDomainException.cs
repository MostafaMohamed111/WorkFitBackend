

using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Exceptions;

internal sealed class DuplicateAssessmentSkillChangeDomainException : DomainException
{
    public DuplicateAssessmentSkillChangeDomainException(string skillName) : base(
            ModuleMarker.ModuleName,
            "DUPLICATE_ASSESSMENT_SKILL_CHANGE",
            $"Duplicate skill change for the skill '{skillName}' is not allowed."
        )
    {
        
    }
}
