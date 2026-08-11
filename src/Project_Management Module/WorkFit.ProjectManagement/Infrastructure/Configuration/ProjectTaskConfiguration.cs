
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectTaskConfiguration
    : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        // Mark the table as trigger-backed so EF does not rely on rowcount-based
        // concurrency checks that can fail when SQL Server triggers are present.
        builder.ToTable("tasks", tb => tb.HasTrigger("TR_tasks"));

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
               .HasConversion<string>();

        builder.Property(x => x.SourceReferenceId)
               .HasMaxLength(255);

        builder.Property(x => x.GitHubBranchName)
               .HasMaxLength(255);

        builder.Property(x => x.GitHubBranchNodeId)
               .HasMaxLength(255);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
