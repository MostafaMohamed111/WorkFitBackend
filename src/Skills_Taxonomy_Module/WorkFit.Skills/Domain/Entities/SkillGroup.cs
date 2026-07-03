using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillGroup : BaseEntity
{
    // === Properties ===
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }

    // === Navigation ===
    public SkillCategory Category { get; private set; }
    public ICollection<Skill> Skills { get; private set; } = new List<Skill>();

    // === Private Constructor (EF Core) ===
    private SkillGroup() { }

    // === Factory Method ===
    public static SkillGroup Create(
        string name,
        Guid categoryId,
        string? description = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(SkillGroup),
                nameof(name)
            );

        return new SkillGroup
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            CategoryId = categoryId,
            DisplayOrder = displayOrder,
            IsActive = true,
        };
    }

    // === Domain Methods ===
    public void Update(
        string name,
        string? description,
        int displayOrder,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(SkillGroup),
                nameof(name)
            );

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}