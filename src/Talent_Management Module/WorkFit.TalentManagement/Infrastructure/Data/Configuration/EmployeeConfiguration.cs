using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.JobTitle).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.OrganizationId);


        builder.HasMany(x => x.EmployeeSkills)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Certifications)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}