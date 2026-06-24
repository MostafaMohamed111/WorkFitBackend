using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;

namespace WorkFit.Organizations.Infrastructure.Data
{
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
        }
    }
}
