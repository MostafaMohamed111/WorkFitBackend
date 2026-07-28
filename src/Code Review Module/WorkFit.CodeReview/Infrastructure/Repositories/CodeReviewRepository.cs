using Microsoft.EntityFrameworkCore;
using WorkFit.CodeReview.Domain.Entities;
using WorkFit.CodeReview.Infrastructure.Data;

namespace WorkFit.CodeReview.Infrastructure.Repositories;

public sealed class CodeReviewRepository : ICodeReviewRepository
{
    private readonly CodeReviewDbContext _context;

    public CodeReviewRepository(CodeReviewDbContext context)
    {
        _context = context;
    }

    public Task<RepoMetadataCacheEntry?> GetFreshRepoMetadataAsync(string cacheKey, DateTime utcNow, TimeSpan ttl, CancellationToken ct)
    {
        return _context.RepoMetadataCacheEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CacheKey == cacheKey && x.CachedAt >= utcNow.Subtract(ttl),
                ct);
    }

    public async Task UpsertRepoMetadataAsync(RepoMetadataCacheEntry entry, CancellationToken ct)
    {
        var existing = await _context.RepoMetadataCacheEntries
            .FirstOrDefaultAsync(x => x.CacheKey == entry.CacheKey, ct);

        if (existing is null)
        {
            await _context.RepoMetadataCacheEntries.AddAsync(entry, ct);
            return;
        }

        existing.Update(entry.DefaultBranch, entry.MetadataJson, entry.CachedAt);
    }

    public async Task AddSuccessLogAsync(CodeReviewRunLogEntry entry, CancellationToken ct)
    {
        await _context.CodeReviewRunLogEntries.AddAsync(entry, ct);
    }

    public async Task AddFailureLogAsync(CodeReviewRunLogEntry entry, CancellationToken ct)
    {
        await _context.CodeReviewRunLogEntries.AddAsync(entry, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
