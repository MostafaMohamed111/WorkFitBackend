

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectActivityLogConfiguration
    : IEntityTypeConfiguration<ProjectActivityLog>
{
    public void Configure(EntityTypeBuilder<ProjectActivityLog> builder)
    {
        builder.ToTable("project_activity_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.EntityType)
               .HasConversion<string>();

        builder.Property(x => x.CreatedAt)
               .IsRequired();
    }
}