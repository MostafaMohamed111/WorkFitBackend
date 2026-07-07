using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
    {
        builder.HasKey(x => x.Id);

        // الموظف مينفعش تتكرر عنده نفس المهارة
        builder.HasIndex(x => new { x.EmployeeProfileId, x.SkillId }).IsUnique();
    }
}