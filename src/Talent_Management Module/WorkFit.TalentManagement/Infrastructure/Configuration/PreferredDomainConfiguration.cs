using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Configuration;

public class PreferredDomainConfiguration : IEntityTypeConfiguration<PreferredDomain>
{
    public void Configure(EntityTypeBuilder<PreferredDomain> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.EmployeeId, x.Order });
    }
}