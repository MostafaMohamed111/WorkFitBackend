
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.Status)
               .HasConversion<string>();

        builder.Property(x => x.StartDate);

        builder.Property(x => x.EndDate);

        builder.HasMany(x => x.Tasks)
               .WithOne(x => x.Project)
               .HasForeignKey(x => x.ProjectId);

        builder.HasMany(x => x.Assignments)
               .WithOne(x => x.Project)
               .HasForeignKey(x => x.ProjectId);

        builder.HasMany(x => x.ActivityLogs)
               .WithOne(x => x.Project)
               .HasForeignKey(x => x.ProjectId);

        builder.HasMany(x => x.RequiredSkills)
       .WithOne(x => x.Project)
       .HasForeignKey(x => x.ProjectId);
    }
}
