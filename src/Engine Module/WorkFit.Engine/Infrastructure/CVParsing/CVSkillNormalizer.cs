using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Skills.Contracts;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public interface ICVSkillNormalizer
{
    Task<IReadOnlyList<NormalizedSkill>> NormalizeAsync(IReadOnlyList<ParsedSkill> parsedSkills, CancellationToken ct = default);
}

public sealed record NormalizedSkill(Guid SkillId, string SkillName, int ConfidenceScore, string? Evidence, string Source);

public sealed class CVSkillNormalizer : ICVSkillNormalizer
{
    private readonly ISkillCatalog _skillCatalog;

    public CVSkillNormalizer(ISkillCatalog skillCatalog) => _skillCatalog = skillCatalog;

    public async Task<IReadOnlyList<NormalizedSkill>> NormalizeAsync(IReadOnlyList<ParsedSkill> parsedSkills, CancellationToken ct = default)
    {
        // First pass — resolve each parsed skill to a canonical SkillId via Skills module (exact / synonym / embedding-similarity).
        var resolved = new List<NormalizedSkill>();
        // Group identical raw tokens so we don't call the catalog twice for "React" and "React".
        var byToken = parsedSkills
            .GroupBy(s => (s.Name ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        foreach (var p in byToken)
        {
            if (string.IsNullOrWhiteSpace(p.Name)) continue;
            var r = await _skillCatalog.ResolveOrCreateSkillAsync(p.Name, ct);
            resolved.Add(new NormalizedSkill(r.SkillId, r.Name, p.ConfidenceScore, p.Evidence, "CV"));
        }
        return resolved;
    }
}
