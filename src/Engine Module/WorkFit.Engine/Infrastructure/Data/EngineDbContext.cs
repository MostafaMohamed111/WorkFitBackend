using Microsoft.EntityFrameworkCore;

namespace WorkFit.Engine.Infrastructure.Data;

public sealed class EngineDbContext : DbContext
{
    public EngineDbContext(DbContextOptions<EngineDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EngineDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
