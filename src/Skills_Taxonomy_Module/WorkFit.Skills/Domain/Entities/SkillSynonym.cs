// Domain/Entities/SkillSynonym.cs
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillSynonym : BaseEntity
{
    public Guid SkillId { get; private set; }
    public string AliasText { get; private set; } = default!;
    public string NormalizedAlias { get; private set; } = default!;

    private SkillSynonym() { } 

    private SkillSynonym(Guid skillId, string aliasText, string normalizedAlias)
    {
        SkillId = skillId;
        AliasText = aliasText;
        NormalizedAlias = normalizedAlias;
    }

    internal static SkillSynonym Create(Guid skillId, string aliasText)
        => new(skillId, aliasText.Trim(), Skill.Normalize(aliasText));
}