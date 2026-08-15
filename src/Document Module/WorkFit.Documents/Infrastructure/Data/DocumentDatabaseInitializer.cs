using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkFit.Documents.Infrastructure.Data;

public sealed class DocumentDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentDatabaseInitializer> _logger;

    public DocumentDatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<DocumentDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();

        _logger.LogInformation("Applying Document module database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Document module database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
