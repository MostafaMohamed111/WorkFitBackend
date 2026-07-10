using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal class DeveloperIdentityMappingConfiguration : IEntityTypeConfiguration<DeveloperIdentityMapping>
{
    public void Configure(EntityTypeBuilder<DeveloperIdentityMapping> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceSystem).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ExternalAccountId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ExternalDisplayName).IsRequired().HasMaxLength(255);

        builder.HasIndex(x => new { x.SourceSystem, x.ExternalAccountId }).IsUnique();
    }
}
