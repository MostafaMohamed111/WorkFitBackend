using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class CannotDeleteSystemSkillException : FeatureException
{
    public CannotDeleteSystemSkillException(Guid skillId)
        : base(
            ModuleMarker.ModuleName,
            "CANNOT_DELETE_SYSTEM_SKILL",
            $"Cannot delete system skill with ID '{skillId}'. System skills are read-only.",
            "System skills cannot be deleted. They are platform-wide and read-only."
        )
    {
    }
}