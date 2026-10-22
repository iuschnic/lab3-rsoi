using Gateway.Application.Operations;

namespace Gateway.Application.OutPorts;

public interface IDeferredOperationScheduler
{
    // Completion means the continuation has been accepted by the scheduler.
    Task ScheduleAsync(
        DeferredOperation operation,
        DateTimeOffset executeAfter,
        CancellationToken cancellationToken = default);
}
