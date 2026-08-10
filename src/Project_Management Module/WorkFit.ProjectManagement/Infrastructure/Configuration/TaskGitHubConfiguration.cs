using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public sealed class TaskGitHubConfiguration : IEntityTypeConfiguration<TaskGitHub>
{
    public void Configure(EntityTypeBuilder<TaskGitHub> builder)
    {
        builder.ToTable("task_github");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GitHubRepositoryName).HasMaxLength(200);
        builder.Property(x => x.GitHubBranchName).HasMaxLength(255);
        builder.Property(x => x.CompletedAt).HasColumnType("datetimeoffset");

        builder.HasIndex(x => x.TaskId).IsUnique();

        builder.HasOne(x => x.Task)
            .WithOne()
            .HasForeignKey<TaskGitHub>(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
