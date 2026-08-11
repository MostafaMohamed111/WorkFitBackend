using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.CodeReview.Domain.Entities;

namespace WorkFit.CodeReview.Infrastructure.Data.Configurations;

public sealed class CodeReviewRunLogEntryConfiguration : IEntityTypeConfiguration<CodeReviewRunLogEntry>
{
    public void Configure(EntityTypeBuilder<CodeReviewRunLogEntry> builder)
    {
        builder.ToTable("code_review_log");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExecutionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TaskId);
        builder.Property(x => x.EmployeeId);
        builder.Property(x => x.Organization).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Repository).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Branch).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CommitSha).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PullRequestNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OverallScore).IsRequired();
        builder.Property(x => x.Risk).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.LoggedAt).IsRequired();

        builder.HasIndex(x => x.ExecutionId);
        builder.HasIndex(x => x.TaskId);
        builder.HasIndex(x => x.EmployeeId);
    }
}
