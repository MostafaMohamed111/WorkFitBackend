using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data;

public class TalentDbContext : DbContext
{
    public TalentDbContext(DbContextOptions<TalentDbContext> options)
        : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TalentProfile> TalentProfiles => Set<TalentProfile>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<SkillEvidence> SkillEvidences => Set<SkillEvidence>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<PreferredDomain> PreferredDomains => Set<PreferredDomain>();
    public DbSet<WorkHistory> WorkHistories => Set<WorkHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // كل الجداول هتبقى تحت schema اسمه "talent"
        builder.HasDefaultSchema("talent");

        // EF Core هيلاقي كل كلاسات الـ Configuration في نفس الـ assembly تلقائي
        builder.ApplyConfigurationsFromAssembly(typeof(TalentDbContext).Assembly);
    }
}