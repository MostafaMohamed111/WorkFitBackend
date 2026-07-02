using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Configuration;

public class WorkHistoryConfiguration : IEntityTypeConfiguration<WorkHistory>
{
    public void Configure(EntityTypeBuilder<WorkHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.ProjectId);
    }
}