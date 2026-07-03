using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.Skills.Domain.Exceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillPrerequisite : BaseEntity
{
    // === Properties ===
    public Guid SkillId { get; private set; }
    public Guid PrerequisiteSkillId { get; private set; }
    public bool IsRequired { get; private set; }

    // === Navigation ===
    public Skill Skill { get; private set; }
    public Skill PrerequisiteSkill { get; private set; }

    // === Private Constructor (EF Core) ===
    private SkillPrerequisite() { }

    // === Factory Method ===
    public static SkillPrerequisite Create(
        Guid skillId,
        Guid prerequisiteSkillId,
        bool isRequired = true)
    {
        if (skillId == prerequisiteSkillId)
            throw new CircularDependencyException(skillId);

        return new SkillPrerequisite
        {
            SkillId = skillId,
            PrerequisiteSkillId = prerequisiteSkillId,
            IsRequired = isRequired,
        };
    }
}