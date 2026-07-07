using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data;

public class TalentDbContext : DbContext
{
    public TalentDbContext(DbContextOptions<TalentDbContext> options)
        : base(options) { }

    internal DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    internal DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    internal DbSet<ConfidenceEvidence> SkillEvidences => Set<ConfidenceEvidence>();
    internal DbSet<Certification> Certifications => Set<Certification>();
    internal DbSet<SkillConfidenceChange> SkillConfidenceChanges => Set<SkillConfidenceChange>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // كل الجداول هتبقى تحت schema اسمه "talent"
        builder.HasDefaultSchema("talent");

        // EF Core هيلاقي كل كلاسات الـ Configuration في نفس الـ assembly تلقائي
        builder.ApplyConfigurationsFromAssembly(typeof(TalentDbContext).Assembly);
    }
}