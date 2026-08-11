using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkFit.ProjectManagement.Infrastructure.Data;

public sealed class ProjectManagementDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProjectManagementDatabaseInitializer> _logger;

    public ProjectManagementDatabaseInitializer(
        IServiceProvider serviceProvider,
        ILogger<ProjectManagementDatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkFitProjectDbContext>();

        _logger.LogInformation("Applying project management database migrations.");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Project management database migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
