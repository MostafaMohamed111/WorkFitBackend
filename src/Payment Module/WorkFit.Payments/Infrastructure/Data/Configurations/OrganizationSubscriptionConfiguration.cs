using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.Payments.Domain.Entities;

namespace WorkFit.Payments.Infrastructure.Data.Configurations;

public sealed class OrganizationSubscriptionConfiguration : IEntityTypeConfiguration<OrganizationSubscription>
{
    public void Configure(EntityTypeBuilder<OrganizationSubscription> builder)
    {
        builder.ToTable("organization_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.PlanName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsRecurring)
            .IsRequired();

        builder.Property(x => x.BillingCycle)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.ActivatedAt);

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.PaymentId);

        builder.HasIndex(x => x.OrganizationId)
            .IsUnique();
    }
}
