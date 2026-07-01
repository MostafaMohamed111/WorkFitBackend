
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Configuration;

public class ProjectDomainConfiguration
    : IEntityTypeConfiguration<ProjectDomain>
{
    public void Configure(EntityTypeBuilder<ProjectDomain> builder)
    {
        builder.ToTable("project_domains");

        builder.HasKey(x => new
        {
            x.ProjectId,
            x.DomainId
        });
    }
}