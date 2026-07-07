using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal class SkillEvidenceConfiguration : IEntityTypeConfiguration<ConfidenceEvidence>
{
    public void Configure(EntityTypeBuilder<ConfidenceEvidence> builder)
    {
        builder.HasKey(x => x.Id);
    }
}