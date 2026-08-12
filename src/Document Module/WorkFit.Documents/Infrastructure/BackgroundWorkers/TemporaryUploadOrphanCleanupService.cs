using WorkFit.Documents.Domain.Entities;
using WorkFit.Documents.Infrastructure.Abstractions;
using WorkFit.Documents.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkFit.Documents.Infrastructure.Configuration;

namespace WorkFit.Documents.Infrastructure.BackgroundWorkers;

/// <summary>
/// Eligible rows: <see cref="DocumentStatus.Temporary"/> and <see cref="Document.CreatedAt"/> at least one hour ago,
/// matching the domain rule used by <see cref="Document.MarkAsDeleted"/> for temporary documents.
/// </summary>
internal sealed class TemporaryUploadOrphanCleanupService : ITemporaryUploadOrphanCleanupService
{
    private readonly DocumentDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IOptionsMonitor<TemporaryUploadCleanupOptions> _options;

    public TemporaryUploadOrphanCleanupService(
        DocumentDbContext context,
        IFileStorage fileStorage,
        IOptionsMonitor<TemporaryUploadCleanupOptions> options)
    {
        _context = context;
        _fileStorage = fileStorage;
        _options = options;
    }

    public async Task<int> PurgeStaleTemporaryDocumentsAsync(CancellationToken cancellationToken)
    {
        var maxBatch = Math.Max(1, _options.CurrentValue.MaxBatchSize);
        var cutoff = DateTime.UtcNow.AddHours(-1);

        var batch = await _context.Documents
            .Where(d => d.DocumentStatus == DocumentStatus.Temporary && d.CreatedAt <= cutoff)
            .OrderBy(d => d.CreatedAt)
            .Take(maxBatch)
            .ToListAsync(cancellationToken);

        if (batch.Count == 0)
            return 0;

        foreach (var document in batch)
            await _fileStorage.DeleteAsync(document.StorageKey, cancellationToken);

        _context.Documents.RemoveRange(batch);
        await _context.SaveChangesAsync(cancellationToken);

        return batch.Count;
    }
}
