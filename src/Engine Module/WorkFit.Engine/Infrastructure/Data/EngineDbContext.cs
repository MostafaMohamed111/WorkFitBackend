using Microsoft.EntityFrameworkCore;
using WorkFit.Engine.Domain.Entities;

namespace WorkFit.Engine.Infrastructure.Data;

public sealed class EngineDbContext : DbContext
{
    public EngineDbContext(DbContextOptions<EngineDbContext> options) : base(options) { }

    public DbSet<CVParseJob> CVParseJobs => Set<CVParseJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EngineDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
