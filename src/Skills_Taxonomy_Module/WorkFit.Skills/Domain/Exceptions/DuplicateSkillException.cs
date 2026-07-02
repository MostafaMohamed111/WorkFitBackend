using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Skills.Domain.Exceptions;

public sealed class DuplicateSkillException : FeatureException
{
    public DuplicateSkillException(string name)
        : base(
            ModuleMarker.ModuleName,
            "DUPLICATE_SKILL",
            $"Skill '{name}' already exists in this organization.",
            $"Skill '{name}' already exists. Please use a different name."
        )
    {
    }
}