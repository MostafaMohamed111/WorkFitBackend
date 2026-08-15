using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ProjectId, x.EmployeeProfileId }).IsUnique();
        builder.HasOne(x => x.Project).WithMany(x => x.Members).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}
