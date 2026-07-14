

using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Entities;

namespace WorkFit.Assessments.Infrastructure.Data;

internal sealed class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options)
    {
        
    }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<SkillChange> SkillChanges { get; set; }
}
