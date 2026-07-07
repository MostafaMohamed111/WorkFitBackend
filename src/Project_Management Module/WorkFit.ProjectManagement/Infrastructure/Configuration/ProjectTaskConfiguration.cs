
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectTaskConfiguration
    : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.TaskType)
               .HasConversion<string>();

        builder.Property(x => x.Status)
               .HasConversion<string>();

        builder.Property(x => x.Priority)
               .HasConversion<string>();

        builder.Property(x => x.SourceSystem)
               .HasMaxLength(50);

        builder.Property(x => x.SourceReferenceId)
               .HasMaxLength(255);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}