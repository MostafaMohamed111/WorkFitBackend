using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Skills.Domain.Entities;

public sealed class Skill : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string NormalizedName { get; private set; } = default!;
    public Guid? CategoryId { get; private set; }

    private readonly List<SkillSynonym> _synonyms = new();
    public IReadOnlyCollection<SkillSynonym> Synonyms => _synonyms.AsReadOnly();

    private Skill() { } 

    private Skill(string name, string normalizedName, Guid? categoryId)
    {
        Name = name;
        NormalizedName = normalizedName;
        CategoryId = categoryId;
    }

    public static Skill Create(string name, Guid? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException("Skills", nameof(Skill), nameof(Name));

        return new Skill(name.Trim(), Normalize(name), categoryId);
    }

    public static string Normalize(string raw) => raw.Trim().ToUpperInvariant();


    public void AddSynonym(string aliasText)
    {
        var normalizedAlias = Normalize(aliasText);
        if (_synonyms.Any(s => s.NormalizedAlias == normalizedAlias))
            return;

        _synonyms.Add(SkillSynonym.Create(Id, aliasText));
        MarkUpdated();
    }

    public void AssignCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        MarkUpdated();
    }
}