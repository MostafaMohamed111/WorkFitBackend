using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Configuration;

public class TalentProfileConfiguration : IEntityTypeConfiguration<TalentProfile>
{
    public void Configure(EntityTypeBuilder<TalentProfile> builder)
    {
        builder.HasKey(x => x.Id);
    }
}