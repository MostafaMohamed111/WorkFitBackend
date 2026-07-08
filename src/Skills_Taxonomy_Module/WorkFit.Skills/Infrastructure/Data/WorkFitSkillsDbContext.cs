using Microsoft.EntityFrameworkCore;
using WorkFit.Skills.Domain.Entities;

namespace WorkFit.Skills.Infrastructure.Data;

public sealed class WorkFitSkillsDbContext : DbContext
{
    public WorkFitSkillsDbContext(DbContextOptions<WorkFitSkillsDbContext> options) : base(options) { }

    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillSynonym> SkillSynonyms => Set<SkillSynonym>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkFitSkillsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}