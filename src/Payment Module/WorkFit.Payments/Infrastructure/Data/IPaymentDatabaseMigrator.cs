namespace WorkFit.Payments.Infrastructure.Data;

public interface IPaymentDatabaseMigrator
{
    Task EnsureMigratedAsync(CancellationToken cancellationToken = default);
}
