using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillSynonym : BaseEntity
{
    // === Properties ===
    public Guid SkillId { get; private set; }
    public string Text { get; private set; }
    public string NormalizedText { get; private set; }
    public bool IsSystem { get; private set; }

    // === Navigation ===
    public Skill Skill { get; private set; }

    // === Private Constructor (EF Core) ===
    private SkillSynonym() { }

    // === Factory Method ===
    public static SkillSynonym Create(
        Guid skillId,
        string text,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(SkillSynonym),
                nameof(text)
            );

        return new SkillSynonym
        {
            Id = Guid.NewGuid(),
            SkillId = skillId,
            Text = text.Trim(),
            NormalizedText = text.Trim().ToUpperInvariant(),
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}