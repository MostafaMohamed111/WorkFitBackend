using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Domain.Entities;

namespace WorkFit.Integration.Infrastructure.Data;

public class IntegrationDbContext : DbContext
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

        modelBuilder.Entity<OrganizationIntegrationSetting>(builder =>
        {
            builder.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            builder.Property(x => x.BaseUrl).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ApiToken).IsRequired().HasMaxLength(500);
            builder.Property(x => x.ProjectKey).IsRequired().HasMaxLength(50);

            // Each organization may have at most one setting per provider
            builder.HasIndex(x => new { x.OrganizationId, x.Provider })
                   .IsUnique();
        });
    }
}
