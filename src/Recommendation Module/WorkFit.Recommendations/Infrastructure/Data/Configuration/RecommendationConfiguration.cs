using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Recommendations.Domain.Entities;

namespace WorkFit.Recommendations.Infrastructure.Data.Configuration;

internal class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("recommendations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TaskId).IsRequired();
        builder.Property(x => x.GeneratedAt).IsRequired();
        builder.Property(x => x.RequiredSkillsSnapshot)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasMany(x => x.Candidates)
            .WithOne()
            .HasForeignKey(x => x.RecommendationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TaskId);
        builder.HasIndex(x => x.GeneratedAt);
    }
}