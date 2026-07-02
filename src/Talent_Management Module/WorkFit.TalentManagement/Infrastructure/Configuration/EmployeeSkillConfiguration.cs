using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Configuration;

public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
    {
        builder.HasKey(x => x.Id);

        // الموظف مينفعش تتكرر عنده نفس المهارة
        builder.HasIndex(x => new { x.EmployeeId, x.SkillId }).IsUnique();

        // خزّني الـ enum كـ string مش رقم
        builder.Property(x => x.Proficiency).HasConversion<string>();

        builder.HasMany(x => x.Evidences)
            .WithOne(x => x.EmployeeSkill)
            .HasForeignKey(x => x.EmployeeSkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}