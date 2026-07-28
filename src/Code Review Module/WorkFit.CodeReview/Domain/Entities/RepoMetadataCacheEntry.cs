using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.CodeReview.Domain.Entities;

public sealed class RepoMetadataCacheEntry : BaseEntity
{
    public string CacheKey { get; private set; } = string.Empty;
    public string Organization { get; private set; } = string.Empty;
    public string Repository { get; private set; } = string.Empty;
    public string DefaultBranch { get; private set; } = string.Empty;
    public string MetadataJson { get; private set; } = string.Empty;
    public DateTime CachedAt { get; private set; }

    private RepoMetadataCacheEntry() : base()
    {
    }

    public static RepoMetadataCacheEntry Create(
        string cacheKey,
        string organization,
        string repository,
        string defaultBranch,
        string metadataJson,
        DateTime cachedAt)
    {
        return new RepoMetadataCacheEntry
        {
            CacheKey = cacheKey,
            Organization = organization,
            Repository = repository,
            DefaultBranch = defaultBranch,
            MetadataJson = metadataJson,
            CachedAt = cachedAt
        };
    }

    public void Update(
        string defaultBranch,
        string metadataJson,
        DateTime cachedAt)
    {
        DefaultBranch = defaultBranch;
        MetadataJson = metadataJson;
        CachedAt = cachedAt;
        MarkUpdated();
    }
}
