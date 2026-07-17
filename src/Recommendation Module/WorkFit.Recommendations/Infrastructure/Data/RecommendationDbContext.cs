using Microsoft.EntityFrameworkCore;
using WorkFit.Recommendations.Domain.Entities;

namespace WorkFit.Recommendations.Infrastructure.Data;

public class RecommendationDbContext : DbContext
{
    public RecommendationDbContext(DbContextOptions<RecommendationDbContext> options)
        : base(options) { }

    internal DbSet<Recommendation> Recommendations => Set<Recommendation>();
    internal DbSet<RecommendationCandidate> RecommendationCandidates => Set<RecommendationCandidate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("recommendation");
        builder.ApplyConfigurationsFromAssembly(typeof(RecommendationDbContext).Assembly);
    }
}