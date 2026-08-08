using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.Engine.Infrastructure.Options;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public sealed class BackgroundCVWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly CVProcessingChannel _channel;
    private readonly IOptions<CVParsingOptions> _options;
    private readonly ILogger<BackgroundCVWorker> _logger;

    public BackgroundCVWorker(
        IServiceProvider services,
        CVProcessingChannel channel,
        IOptions<CVParsingOptions> options,
        ILogger<BackgroundCVWorker> logger)
    {
        _services = services;
        _channel = channel;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundCVWorker starting. Concurrency={Concurrency}.", _options.Value.Concurrency);
        var concurrency = Math.Max(1, _options.Value.Concurrency);
        var sem = new SemaphoreSlim(concurrency);
        var runningTasks = new List<Task>();

        try
        {
            await foreach (var cmd in _channel.ReadAllAsync(stoppingToken))
            {
                await sem.WaitAsync(stoppingToken);
                var t = Task.Run(async () =>
                {
                    using var scope = _services.CreateScope();
                    try
                    {
                        await ProcessWithHeartbeatAsync(scope.ServiceProvider, cmd.JobId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Worker exception for job {JobId}.", cmd.JobId);
                    }
                    finally { sem.Release(); }
                }, stoppingToken);
                runningTasks.Add(t);
                runningTasks.RemoveAll(x => x.IsCompleted);
            }
            await Task.WhenAll(runningTasks);
        }
        catch (OperationCanceledException) { }
        _logger.LogInformation("BackgroundCVWorker stopped.");
    }

    private static async Task ProcessWithHeartbeatAsync(IServiceProvider sp, Guid jobId, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var db = sp.GetRequiredService<EngineDbContext>();
        var pipeline = sp.GetRequiredService<ICVParsePipeline>();
        var logger = sp.GetRequiredService<ILogger<BackgroundCVWorker>>();
        var heartbeatMs = 30_000;

        var heartbeatScope = sp.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var heartbeatDb = heartbeatScope.ServiceProvider.GetRequiredService<EngineDbContext>();

        var heartbeatTask = Task.Run(async () =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(heartbeatMs, cts.Token);
                    var j = await heartbeatDb.CVParseJobs.FirstOrDefaultAsync(x => x.Id == jobId, cts.Token);
                    if (j is not null)
                    {
                        j.Heartbeat();
                        await heartbeatDb.SaveChangesAsync(cts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                heartbeatScope.Dispose();
            }
        }, cts.Token);

        await pipeline.ExecuteAsync(jobId, ct);
        cts.Cancel();
        await heartbeatTask;
    }

    public static async Task RequeueStrandedAsync(IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EngineDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<CVParsingOptions>>().Value;
        var cutoff = DateTime.UtcNow.AddSeconds(-options.HeartbeatTimeoutSeconds);
        var stranded = await db.CVParseJobs
            .Where(j => j.Status == "Processing" && j.HeartbeatAt != null && j.HeartbeatAt < cutoff)
            .ToListAsync(ct);
        foreach (var j in stranded) j.Requeue();
        if (stranded.Count > 0) await db.SaveChangesAsync(ct);
    }
}
