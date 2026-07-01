using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;
    public class ProjectRequiredSkillConfiguration
    : IEntityTypeConfiguration<ProjectRequiredSkill>
    {
        public void Configure(EntityTypeBuilder<ProjectRequiredSkill> builder)
        {
            builder.ToTable("ProjectRequiredSkills");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Level)
                   .HasConversion<string>();

            builder.HasOne(x => x.Project)
                   .WithMany(x => x.RequiredSkills)
                   .HasForeignKey(x => x.ProjectId);
        }
}
