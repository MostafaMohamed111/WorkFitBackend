using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Configuration;

public class SkillEvidenceConfiguration : IEntityTypeConfiguration<SkillEvidence>
{
    public void Configure(EntityTypeBuilder<SkillEvidence> builder)
    {
        builder.HasKey(x => x.Id);
    }
}