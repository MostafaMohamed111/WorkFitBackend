using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class SkillCategory : BaseEntity
{
    public string Name { get; private set; } = default!;

    private SkillCategory() { }

    private SkillCategory(string name) => Name = name;

    public static SkillCategory Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException("Skills", nameof(SkillCategory), nameof(Name));

        return new SkillCategory(name.Trim());
    }
}