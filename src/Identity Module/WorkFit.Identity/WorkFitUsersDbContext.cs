

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkFit.Identity.Domain.Entities;

namespace WorkFit.Identity.Infrastructure.Data
{
    internal class WorkFitUsersDbContext : IdentityDbContext<WorkFitUser, WorkFitRole, Guid>
    {
        public WorkFitUsersDbContext(DbContextOptions<WorkFitUsersDbContext> options) : base(options)
        {

        }

        public DbSet<WorkFitUser> WorkFitUsers { get; set; }

        public DbSet<WorkFitRole> WorkFitRoles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Identity");


        }
    }
}
