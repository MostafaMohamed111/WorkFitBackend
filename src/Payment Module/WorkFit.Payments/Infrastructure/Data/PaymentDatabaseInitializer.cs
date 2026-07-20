using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkFit.Payments.Infrastructure.Data;

public sealed class PaymentDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentDatabaseInitializer> _logger;

    public PaymentDatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<PaymentDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        _logger.LogInformation("Applying payment database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Payment database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
