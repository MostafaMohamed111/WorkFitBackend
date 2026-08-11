namespace WorkFit.Documents.Infrastructure.BackgroundWorkers;

/// <summary>
/// Purges stale temporary documents: same age rule as <see cref="Domain.Entities.Document"/> (not attached, created at least one hour ago).
/// </summary>
public interface ITemporaryUploadOrphanCleanupService
{
    /// <summary>Deletes physical files and database rows for one batch. Returns how many were removed.</summary>
    Task<int> PurgeStaleTemporaryDocumentsAsync(CancellationToken cancellationToken);
}
