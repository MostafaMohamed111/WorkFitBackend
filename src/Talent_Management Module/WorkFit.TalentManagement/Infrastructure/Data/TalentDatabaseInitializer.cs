using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkFit.TalentManagement.Infrastructure.Data;

public sealed class TalentDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TalentDatabaseInitializer> _logger;

    public TalentDatabaseInitializer(IServiceProvider serviceProvider, ILogger<TalentDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TalentDbContext>();
        _logger.LogInformation("Applying Talent Management database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Talent Management database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
