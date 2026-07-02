using Microsoft.EntityFrameworkCore;
using URL_Shortener.Models;
using URL_Shortener.Models.Data;
using URL_Shortener.Services;

namespace URL_Shortener.Services;

public class ClickLogProcessorService : BackgroundService
{
    private readonly ClickLogQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClickLogProcessorService> _logger;
    private const int MaxBatchSize = 5;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    public ClickLogProcessorService(
        ClickLogQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ClickLogProcessorService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<ClickLog>(MaxBatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();
            var flushDeadline = Task.Delay(FlushInterval, stoppingToken);

            while (batch.Count < MaxBatchSize)
            {
                var readTask = _queue.Reader.WaitToReadAsync(stoppingToken).AsTask();

                if (await Task.WhenAny(readTask, flushDeadline) == flushDeadline)
                    break;

                while (batch.Count < MaxBatchSize && _queue.Reader.TryRead(out var clickLog))
                {
                    batch.Add(clickLog);
                }
            }

            if (batch.Count == 0) continue;

            await FlushBatchAsync(batch, stoppingToken);
        }
    }

    private async Task FlushBatchAsync(List<ClickLog> batch, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.ClickLogs.AddRangeAsync(batch, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Flushed {Count} click logs to DB", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch insert failed, retrying individually...");
            await RetryIndividuallyAsync(batch, stoppingToken);
        }
    }

    private async Task RetryIndividuallyAsync(List<ClickLog> batch, CancellationToken stoppingToken)
    {
        foreach (var log in batch)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.ClickLogs.Add(log);
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save individual click log for URL {Id}", log.ShortenedURLId);
            }
        }
    }
}