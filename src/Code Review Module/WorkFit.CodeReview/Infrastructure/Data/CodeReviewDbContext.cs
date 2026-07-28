using Microsoft.EntityFrameworkCore;
using WorkFit.CodeReview.Domain.Entities;

namespace WorkFit.CodeReview.Infrastructure.Data;

public sealed class CodeReviewDbContext : DbContext
{
    public CodeReviewDbContext(DbContextOptions<CodeReviewDbContext> options) : base(options)
    {
    }

    public DbSet<RepoMetadataCacheEntry> RepoMetadataCacheEntries => Set<RepoMetadataCacheEntry>();
    public DbSet<CodeReviewRunLogEntry> CodeReviewRunLogEntries => Set<CodeReviewRunLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CodeReviewDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
