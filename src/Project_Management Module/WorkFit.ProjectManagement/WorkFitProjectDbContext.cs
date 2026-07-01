using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;



namespace WorkFit.ProjectManagement;
public class WorkFitProjectDbContext : DbContext
{
    public WorkFitProjectDbContext(DbContextOptions<WorkFitProjectDbContext> options) : base(options)
    {

    }

    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectTask> ProjectTasks { get; set; }

    public DbSet<ProjectActivityLog> ProjectActivityLogs { get; set; }
    public DbSet<ProjectDomain> ProjectDomains { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }

    public DbSet<ProjectRequiredSkill> ProjectRequiredSkills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ProjectManagement");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WorkFitProjectDbContext).Assembly);
    }


}

