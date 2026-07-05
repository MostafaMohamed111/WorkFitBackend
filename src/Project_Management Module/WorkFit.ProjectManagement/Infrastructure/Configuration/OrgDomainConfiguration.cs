using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.ProjectManagement.Domain.Entities;

namespace WorkFit.ProjectManagement.Infrastructure.Data.Configurations;

public sealed class OrgDomainConfiguration : IEntityTypeConfiguration<OrgDomain>
{
    public void Configure(EntityTypeBuilder<OrgDomain> builder)
    {
        builder.ToTable("domains", "ProjectManagement");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

    }
}