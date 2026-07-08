using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Skills.Domain.Entities;

namespace WorkFit.Skills.Infrastructure.Data.Configurations;

public sealed class SkillSynonymConfiguration : IEntityTypeConfiguration<SkillSynonym>
{
    public void Configure(EntityTypeBuilder<SkillSynonym> builder)
    {
        builder.ToTable("SkillSynonyms");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AliasText).HasMaxLength(150).IsRequired();
        builder.Property(s => s.NormalizedAlias).HasMaxLength(150).IsRequired();

        builder.HasIndex(s => s.NormalizedAlias).IsUnique();
    }
}