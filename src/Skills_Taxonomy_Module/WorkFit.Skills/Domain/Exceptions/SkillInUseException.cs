using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class SkillInUseException : FeatureException
{
    public SkillInUseException(Guid skillId)
        : base(
            ModuleMarker.ModuleName,
            "SKILL_IN_USE",
            $"Skill with ID '{skillId}' is in use and cannot be deleted.",
            "This skill is currently in use and cannot be deleted."
        )
    {
    }
}