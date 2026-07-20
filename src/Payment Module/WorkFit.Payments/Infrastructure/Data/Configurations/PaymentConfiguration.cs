using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Payments.Domain.Entities;

namespace WorkFit.Payments.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Provider)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.ProviderPaymentId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TransactionId)
            .HasMaxLength(200);

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(500);

        builder.HasIndex(x => x.ProviderPaymentId)
            .IsUnique();

        builder.HasIndex(x => new { x.ReferenceId, x.ReferenceType });
    }
}
