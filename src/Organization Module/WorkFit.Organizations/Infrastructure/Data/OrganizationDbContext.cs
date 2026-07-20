using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;

namespace WorkFit.Organizations.Infrastructure.Data;

public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; } = null!;
 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Organization");

        modelBuilder.Entity<Organization>(builder =>
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.BrandingJson).IsRequired();
            builder.Property(x => x.SettingsJson).IsRequired();
        });
    }
}
