using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Infrastructure.Data.Configuration;

internal class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.IssuingOrganization).IsRequired().HasMaxLength(200);

        // IsExpired متحسوبة في C#، مش متخزنة في الداتابيز
        builder.Ignore(x => x.IsExpired);
    }
}