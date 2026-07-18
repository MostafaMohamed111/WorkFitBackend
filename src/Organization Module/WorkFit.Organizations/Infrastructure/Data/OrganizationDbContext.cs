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
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<OrganizationMember> OrganizationMembers { get; set; } = null!;


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

        modelBuilder.Entity<Department>(builder =>
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Team>(builder =>
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrganizationMember>(builder =>
        {
            builder.HasIndex(x => x.UserId).IsUnique();

            builder.HasOne<Organization>()
                .WithMany(o => o.Members)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
