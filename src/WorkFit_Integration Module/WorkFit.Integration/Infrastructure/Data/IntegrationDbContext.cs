using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Domain.Entities;

namespace WorkFit.Integration.Infrastructure.Data;

internal sealed class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrganizationIntegrationSetting> OrganizationIntegrationSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Integration");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationDbContext).Assembly);
    }
}
