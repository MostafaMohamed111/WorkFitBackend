using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillCategory : BaseEntity
{
    // === Properties ===
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    // === Navigation ===
    public ICollection<SkillGroup> Groups { get; private set; } = new List<SkillGroup>();

    // === Private Constructor (EF Core) ===
    private SkillCategory() { }

    // === Factory Method ===
    public static SkillCategory Create(
        string name,
        string? description = null,
        string? icon = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(SkillCategory),
                nameof(name)
            );

        return new SkillCategory
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            Icon = icon,
            DisplayOrder = displayOrder,
            IsActive = true,
        };
    }

    // === Domain Methods ===
    public void Update(
        string name,
        string? description,
        string? icon,
        int displayOrder,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(SkillCategory),
                nameof(name)
            );

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        Icon = icon;
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