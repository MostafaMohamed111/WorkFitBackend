namespace WorkFit.Skills.Infrastructure.Similarity;

public sealed record SkillSimilarityMatch(Guid SkillId, double Confidence);

public interface ISkillSimilarityService
{
    Task<SkillSimilarityMatch?> FindBestMatchAsync(string rawTerm, CancellationToken cancellationToken = default);
}