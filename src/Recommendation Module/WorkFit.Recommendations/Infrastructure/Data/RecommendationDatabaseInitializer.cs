using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkFit.Recommendations.Infrastructure.Data;

public sealed class RecommendationDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecommendationDatabaseInitializer> _logger;

    public RecommendationDatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<RecommendationDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RecommendationDbContext>();
        _logger.LogInformation("Applying Recommendation database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Recommendation database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
