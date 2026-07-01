
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectAssignmentConfiguration
    : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.ToTable("project_assignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleOnProject)
               .HasMaxLength(100);

        builder.Property(x => x.AllocationPercentage)
               .IsRequired();

        builder.Property(x => x.IsActive)
               .HasDefaultValue(true);
    }
}