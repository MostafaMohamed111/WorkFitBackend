using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WorkFit.Payments.Infrastructure.Data;

public sealed class PaymentDatabaseMigrator : IPaymentDatabaseMigrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _migrationLock = new(1, 1);
    private bool _hasMigrated;

    public PaymentDatabaseMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task EnsureMigratedAsync(CancellationToken cancellationToken = default)
    {
        if (_hasMigrated)
        {
            return;
        }

        await _migrationLock.WaitAsync(cancellationToken);
        try
        {
            if (_hasMigrated)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            await context.Database.MigrateAsync(cancellationToken);

            _hasMigrated = true;
        }
        finally
        {
            _migrationLock.Release();
        }
    }
}
