namespace WorkFit.Skills.Infrastructure.Similarity;

/// Placeholder until a real embedding/AI similarity service exists.
/// Always returns null, so ResolveOrCreateSkill falls back to
/// exact/synonym match only, then creates a new skill if nothing hits.
public sealed class NoOpSkillSimilarityService : ISkillSimilarityService
{
    public Task<SkillSimilarityMatch?> FindBestMatchAsync(string rawTerm, CancellationToken cancellationToken = default)
        => Task.FromResult<SkillSimilarityMatch?>(null);
}