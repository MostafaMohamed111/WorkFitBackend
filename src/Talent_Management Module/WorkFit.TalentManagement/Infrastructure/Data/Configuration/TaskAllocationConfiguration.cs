using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal sealed class TaskAllocationConfiguration : IEntityTypeConfiguration<TaskAllocation>
{
    public void Configure(EntityTypeBuilder<TaskAllocation> builder)
    {
        builder.ToTable("TaskAllocations");
        builder.HasKey(x => x.TaskId);
        builder.Property(x => x.TaskId).ValueGeneratedNever();
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Ignore(x => x.ContributesToAllocation);
        builder.HasIndex(x => x.EmployeeProfileId);
    }
}
