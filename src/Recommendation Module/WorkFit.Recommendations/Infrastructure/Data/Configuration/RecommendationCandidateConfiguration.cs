using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Recommendations.Domain.Entities;

namespace WorkFit.Recommendations.Infrastructure.Data.Configuration;

internal class RecommendationCandidateConfiguration : IEntityTypeConfiguration<RecommendationCandidate>
{
    public void Configure(EntityTypeBuilder<RecommendationCandidate> builder)
    {
        builder.ToTable("recommendation_candidates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.MatchScore).HasColumnType("decimal(5,2)");
        builder.Property(x => x.MatchReasoning).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Rank).IsRequired();
        
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        builder.Property(x => x.ReviewedBy);
        builder.Property(x => x.ReviewedAt);

        builder.HasIndex(x => x.RecommendationId);
        builder.HasIndex(x => new { x.RecommendationId, x.Rank });
    }
}