using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Skills.Domain.Entities;

namespace WorkFit.Skills.Infrastructure.Data.Configurations;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.NormalizedName).HasMaxLength(150).IsRequired();
        builder.HasIndex(s => s.NormalizedName).IsUnique();

        builder.HasOne<SkillCategory>()
               .WithMany()
               .HasForeignKey(s => s.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Synonyms)
               .WithOne()
               .HasForeignKey(sy => sy.SkillId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Synonyms).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}