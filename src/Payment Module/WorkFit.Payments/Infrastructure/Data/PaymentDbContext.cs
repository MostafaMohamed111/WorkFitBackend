using Microsoft.EntityFrameworkCore;
using WorkFit.Payments.Domain.Entities;

namespace WorkFit.Payments.Infrastructure.Data;

public sealed class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    internal DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
