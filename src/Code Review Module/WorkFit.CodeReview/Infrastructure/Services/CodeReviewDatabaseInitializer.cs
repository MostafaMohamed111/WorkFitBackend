using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkFit.CodeReview.Infrastructure.Data;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class CodeReviewDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CodeReviewDatabaseInitializer> _logger;

    public CodeReviewDatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<CodeReviewDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CodeReviewDbContext>();

        _logger.LogInformation("Applying CodeReview database migrations.");
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
