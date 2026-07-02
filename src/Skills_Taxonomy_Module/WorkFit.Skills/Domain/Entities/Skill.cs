using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Skills.Domain.Enums;
using WorkFit.Skills.Domain.Exceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class Skill : BaseEntity
{
    // === Properties ===
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? GroupId { get; private set; }
    public Guid? ParentSkillId { get; private set; }
    public SkillOrigin Origin { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public bool IsDeleted { get; private set; }
    public int EstimatedTimeToLearn { get; private set; }
    public string ConfidenceConfigJson { get; private set; } = "{}";

    // === Navigation Properties ===
    public SkillCategory? Category { get; private set; }
    public SkillGroup? Group { get; private set; }
    public Skill? ParentSkill { get; private set; }
    public ICollection<Skill> ChildSkills { get; private set; } = new List<Skill>();
    public ICollection<SkillSynonym> Synonyms { get; private set; } = new List<SkillSynonym>();
    public ICollection<SkillPrerequisite> Prerequisites { get; private set; } = new List<SkillPrerequisite>();
    public ICollection<SkillPrerequisite> RequiredFor { get; private set; } = new List<SkillPrerequisite>();
    public ICollection<SkillRelation> RelatedSkills { get; private set; } = new List<SkillRelation>();
    public ICollection<SkillRelation> RelatedFrom { get; private set; } = new List<SkillRelation>();

    // === Private Constructor (EF Core) ===
    private Skill() { }

    // === Factory Method ===
    public static Skill Create(
        string name,
        string? description,
        SkillOrigin origin,
        Guid? organizationId = null,
        Guid? categoryId = null,
        Guid? groupId = null,
        Guid? parentSkillId = null,
        int estimatedTimeToLearn = 40)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,  
                nameof(Skill),           
                nameof(name)              
            );

        if (origin == SkillOrigin.Custom && !organizationId.HasValue)
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(Skill),
                nameof(organizationId)
            );

        return new Skill
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            Origin = origin,
            OrganizationId = organizationId,
            CategoryId = categoryId,
            GroupId = groupId,
            ParentSkillId = parentSkillId,
            EstimatedTimeToLearn = estimatedTimeToLearn,
            IsDeleted = false,
            ConfidenceConfigJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        Guid? categoryId,
        Guid? groupId,
        int estimatedTimeToLearn)
    {
        if (Origin == SkillOrigin.System)
            throw new CannotModifySystemSkillException(Id);

        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(Skill),
                nameof(name)
            );

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        CategoryId = categoryId;
        GroupId = groupId;
        EstimatedTimeToLearn = estimatedTimeToLearn;
        MarkUpdated();
    }

    public void SoftDelete()
    {
        if (Origin == SkillOrigin.System)
            throw new CannotDeleteSystemSkillException(Id);

        if (IsDeleted)
            return;

        IsDeleted = true;
        MarkUpdated();
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        MarkUpdated();
    }

    public void MoveToCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        MarkUpdated();
    }

    public void MoveToGroup(Guid groupId)
    {
        GroupId = groupId;
        MarkUpdated();
    }

    public void SetParent(Guid? parentSkillId)
    {
        if (parentSkillId == Id)
            throw new CircularDependencyException(Id);

        ParentSkillId = parentSkillId;
        MarkUpdated();
    }
}