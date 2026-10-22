using Gateway.Application.InPorts;
using Gateway.Application.OutPorts;
using Gateway.Application.Services;
using Gateway.Infrastructure.DeferredOperations;

namespace Gateway.Api.BackgroundServices;

public sealed class DeferredOperationWorker(
    IDeferredOperationSource source,
    IDeferredOperationScheduler scheduler,
    IServiceScopeFactory scopeFactory,
    ILogger<DeferredOperationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var operation = await source.ReadAsync(stoppingToken);

                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<IDeferredOperationService>();
                    var remaining = await service.ProcessAsync(operation, stoppingToken);

                    if (remaining is not null)
                    {
                        await scheduler.ScheduleAsync(
                            remaining,
                            DateTimeOffset.UtcNow + CarRentalService.RetryDelay,
                            stoppingToken);
                        logger.LogWarning("Deferred step {Step} is unavailable; retry scheduled", remaining.GetType().Name);
                    }
                    else
                    {
                        logger.LogInformation("Deferred operation {Operation} completed", operation);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    // Do not replay the original command: some steps may already have succeeded.
                    // Availability failures are returned as continuations by the application service.
                    logger.LogError(exception, "Deferred operation {Operation} failed permanently", operation);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }
}
