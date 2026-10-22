using Gateway.Application.Operations;
using Gateway.Application.OutPorts;

namespace Gateway.Infrastructure.DeferredOperations;

// Multiple producers (HTTP requests and worker), one reader (worker).
public sealed class InMemoryDeferredOperationQueue :
    IDeferredOperationScheduler, IDeferredOperationSource, IDisposable
{
    private readonly object _gate = new();
    private readonly PriorityQueue<DeferredOperation, (DateTimeOffset Due, long Sequence)> _pending = new();
    private readonly SemaphoreSlim _changed = new(0, 1);
    private long _sequence;

    public Task ScheduleAsync(
        DeferredOperation operation,
        DateTimeOffset executeAfter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            _pending.Enqueue(operation, (executeAfter, _sequence++));
            // Coalesce notifications; every wake-up rechecks the entire queue.
            if (_changed.CurrentCount == 0)
                _changed.Release();
        }

        return Task.CompletedTask;
    }

    public async ValueTask<DeferredOperation> ReadAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TimeSpan? delay = null;

            lock (_gate)
            {
                if (_pending.TryPeek(out _, out var priority))
                {
                    var remaining = priority.Due - DateTimeOffset.UtcNow;
                    if (remaining <= TimeSpan.Zero)
                        return _pending.Dequeue();

                    // Bound the wait for very distant deadlines and clock changes.
                    delay = remaining < TimeSpan.FromDays(1)
                        ? remaining : TimeSpan.FromDays(1);
                }
            }

            if (delay is { } timeout)
                await _changed.WaitAsync(timeout, cancellationToken);
            else
                await _changed.WaitAsync(cancellationToken);
        }
    }

    public void Dispose() => _changed.Dispose();
}
