using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Engine.Domain.Entities;

namespace WorkFit.Engine.Infrastructure.Data.Configurations;

public sealed class CVParseJobConfiguration : IEntityTypeConfiguration<CVParseJob>
{
    public void Configure(EntityTypeBuilder<CVParseJob> builder)
    {
        builder.ToTable("CVParseJobs", "engine");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Mime).IsRequired().HasMaxLength(128);
        builder.Property(x => x.FileHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ParsedJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ExtractedText).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Error).HasMaxLength(1024);
        builder.Property(x => x.OrganizationId).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.FileHash });
        builder.HasIndex(x => x.BatchId);
        builder.HasIndex(x => x.Status);
    }
}
