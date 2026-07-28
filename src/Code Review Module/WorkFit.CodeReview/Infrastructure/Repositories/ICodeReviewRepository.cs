using WorkFit.CodeReview.Domain.Entities;

namespace WorkFit.CodeReview.Infrastructure.Repositories;

public interface ICodeReviewRepository
{
    Task<RepoMetadataCacheEntry?> GetFreshRepoMetadataAsync(string cacheKey, DateTime utcNow, TimeSpan ttl, CancellationToken ct);
    Task UpsertRepoMetadataAsync(RepoMetadataCacheEntry entry, CancellationToken ct);
    Task AddSuccessLogAsync(CodeReviewRunLogEntry entry, CancellationToken ct);
    Task AddFailureLogAsync(CodeReviewRunLogEntry entry, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
