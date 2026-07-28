using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.CodeReview.Domain.Entities;

namespace WorkFit.CodeReview.Infrastructure.Data.Configurations;

public sealed class RepoMetadataCacheEntryConfiguration : IEntityTypeConfiguration<RepoMetadataCacheEntry>
{
    public void Configure(EntityTypeBuilder<RepoMetadataCacheEntry> builder)
    {
        builder.ToTable("repo_metadata_cache");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CacheKey).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Organization).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Repository).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DefaultBranch).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MetadataJson).IsRequired();
        builder.Property(x => x.CachedAt).IsRequired();

        builder.HasIndex(x => x.CacheKey).IsUnique();
    }
}
