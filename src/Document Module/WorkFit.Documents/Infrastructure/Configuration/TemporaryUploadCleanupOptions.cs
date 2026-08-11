namespace WorkFit.Documents.Infrastructure.Configuration;

/// <summary>Background cleanup for temporary uploads that were never attached (see <see cref="Domain.Entities.Document"/>).</summary>
public sealed class TemporaryUploadCleanupOptions
{
    public const string SectionName = "Documents:TemporaryUploadCleanup";

    public bool Enabled { get; set; } = true;

    /// <summary>Delay before the first cleanup pass after the application starts.</summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Time to wait between cleanup passes when the previous pass found nothing to delete.</summary>
    public TimeSpan IntervalWhenIdle { get; set; } = TimeSpan.FromHours(1);

    /// <summary>When the last pass deleted at least one document, wait this long before the next pass (faster drain of large backlogs).</summary>
    public TimeSpan IntervalWhenWorkDone { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Maximum documents processed per pass (single query and one transaction).</summary>
    public int MaxBatchSize { get; set; } = 500;
}
