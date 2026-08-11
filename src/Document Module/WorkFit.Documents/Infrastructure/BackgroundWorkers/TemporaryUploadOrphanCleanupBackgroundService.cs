using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Documents.Infrastructure.Configuration;

namespace WorkFit.Documents.Infrastructure.BackgroundWorkers;

/// <summary>
/// Periodically removes temporary uploads that exceed the one-hour rule on the <see cref="WorkFit.Documents.Domain.Entities.Document"/> aggregate.
/// </summary>
public sealed class TemporaryUploadOrphanCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TemporaryUploadCleanupOptions> _options;
    private readonly ILogger<TemporaryUploadOrphanCleanupBackgroundService> _logger;

    public TemporaryUploadOrphanCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TemporaryUploadCleanupOptions> options,
        ILogger<TemporaryUploadOrphanCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = _options.CurrentValue;
        if (!opts.Enabled)
        {
            _logger.LogInformation("Temporary upload orphan cleanup is disabled ({Section}).", TemporaryUploadCleanupOptions.SectionName);
            return;
        }

        if (opts.InitialDelay > TimeSpan.Zero)
            await Task.Delay(opts.InitialDelay, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            var o = _options.CurrentValue;
            if (!o.Enabled)
                break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanup = scope.ServiceProvider.GetRequiredService<ITemporaryUploadOrphanCleanupService>();
                var removed = await cleanup.PurgeStaleTemporaryDocumentsAsync(stoppingToken).ConfigureAwait(false);

                if (removed > 0)
                    _logger.LogInformation("Temporary upload cleanup removed {Count} stale document(s).", removed);

                var delay = removed > 0 ? o.IntervalWhenWorkDone : o.IntervalWhenIdle;
                if (delay <= TimeSpan.Zero)
                    delay = TimeSpan.FromHours(1);

                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Temporary upload orphan cleanup failed; retrying after idle interval.");
                var delay = _options.CurrentValue.IntervalWhenIdle;
                if (delay <= TimeSpan.Zero)
                    delay = TimeSpan.FromHours(1);
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
