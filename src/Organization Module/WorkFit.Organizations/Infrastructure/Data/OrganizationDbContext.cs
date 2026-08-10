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
    public DbSet<GitHubAppInstallation> GitHubAppInstallations { get; set; } = null!;
 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Organization");

        modelBuilder.Entity<Organization>(builder =>
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.BrandingJson).IsRequired();
            builder.Property(x => x.SettingsJson).IsRequired();
            builder.Property(x => x.GitHubOrganizationLogin).HasMaxLength(255);
            builder.Property(x => x.GitHubCreatedAt).HasColumnType("datetimeoffset");
        });

        modelBuilder.Entity<GitHubAppInstallation>(builder =>
        {
            builder.Property(x => x.GitHubInstallationId).IsRequired();
            builder.Property(x => x.GitHubOrganizationId).IsRequired();
            builder.Property(x => x.InstalledAt).IsRequired().HasColumnType("datetimeoffset");

            builder.HasIndex(x => x.OrganizationId).IsUnique();
            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
