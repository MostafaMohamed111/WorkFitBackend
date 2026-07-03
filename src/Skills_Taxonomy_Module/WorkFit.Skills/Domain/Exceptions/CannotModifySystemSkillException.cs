using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class CannotModifySystemSkillException : FeatureException
{
    public CannotModifySystemSkillException(Guid skillId)
        : base(
            ModuleMarker.ModuleName,
            "CANNOT_MODIFY_SYSTEM_SKILL",
            $"Cannot modify system skill with ID '{skillId}'. System skills are read-only.",
            "System skills cannot be modified. They are platform-wide and read-only."
        )
    {
    }
}