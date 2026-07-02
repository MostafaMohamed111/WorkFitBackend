using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillRelation : BaseEntity
{
    // === Properties ===
    public Guid SkillId { get; private set; }
    public Guid RelatedSkillId { get; private set; }
    public RelationshipType Type { get; private set; }

    // === Navigation ===
    public Skill Skill { get; private set; }
    public Skill RelatedSkill { get; private set; }

    // === Private Constructor (EF Core) ===
    private SkillRelation() { }

    // === Factory Method ===
    public static SkillRelation Create(
        Guid skillId,
        Guid relatedSkillId,
        RelationshipType type)
    {
        if (skillId == relatedSkillId)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "SELF_RELATION",
                "A skill cannot be related to itself.",
                "Cannot create a relationship between a skill and itself."
            );

        return new SkillRelation
        {
            Id = Guid.NewGuid(),
            SkillId = skillId,
            RelatedSkillId = relatedSkillId,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}