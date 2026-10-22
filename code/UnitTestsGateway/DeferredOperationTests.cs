using Gateway.Api.BackgroundServices;
using Gateway.Application.Exceptions;
using Gateway.Application.InPorts;
using Gateway.Application.Operations;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.DeferredOperations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTestsGateway;

public class DeferredOperationTests
{
    [Fact]
    public async Task EarlierJobWakesReaderWaitingForLaterJob()
    {
        using var queue = new InMemoryDeferredOperationQueue();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var later = new FinishOpReleaseCar(Guid.NewGuid());
        var earlier = new CancelOpCancelPayment(Guid.NewGuid());
        await queue.ScheduleAsync(later, DateTimeOffset.UtcNow.AddMinutes(1));
        var read = queue.ReadAsync(timeout.Token).AsTask();
        Assert.False(read.IsCompleted);

        await queue.ScheduleAsync(earlier, DateTimeOffset.UtcNow);

        Assert.Same(earlier, await read);
    }

    [Fact]
    public async Task FutureJobIsNotDeliveredBeforeDeadline()
    {
        using var queue = new InMemoryDeferredOperationQueue();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var due = DateTimeOffset.UtcNow.AddMilliseconds(150);
        var operation = new FinishOpReleaseCar(Guid.NewGuid());
        await queue.ScheduleAsync(operation, due);

        Assert.Same(operation, await queue.ReadAsync(timeout.Token));
        Assert.True(DateTimeOffset.UtcNow >= due);
    }

    [Fact]
    public async Task EmptyQueueSupportsCancellation()
    {
        using var queue = new InMemoryDeferredOperationQueue();
        using var cancellation = new CancellationTokenSource();
        var read = queue.ReadAsync(cancellation.Token).AsTask();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read);
    }

    [Fact]
    public async Task ConcurrentProducersDoNotLoseOrDuplicateJobs()
    {
        using var queue = new InMemoryDeferredOperationQueue();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var operations = Enumerable.Range(0, 100)
            .Select(_ => new CancelOpCancelPayment(Guid.NewGuid())).ToArray();
        await Task.WhenAll(operations.Select(operation => Task.Run(() =>
            queue.ScheduleAsync(operation, DateTimeOffset.UtcNow))));

        var received = new HashSet<DeferredOperation>();
        foreach (var operation in operations)
            Assert.True(received.Add(await queue.ReadAsync(timeout.Token)));

        Assert.True(operations.All(received.Contains));
    }

    [Fact]
    public async Task WorkerSchedulesReturnedContinuationAndContinuesAfterConflict()
    {
        using var queue = new InMemoryDeferredOperationQueue();
        var scheduler = new RecordingScheduler();
        var processor = new TestProcessor();
        var services = new ServiceCollection();
        services.AddScoped<IDeferredOperationService>(_ => processor);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
        using var worker = new DeferredOperationWorker(queue, scheduler,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DeferredOperationWorker>.Instance);
        await queue.ScheduleAsync(new FinishOpReleaseCar(Guid.NewGuid()), DateTimeOffset.UtcNow);
        var original = new CancelOpCancelRental(Guid.NewGuid(), "user", Guid.NewGuid());
        await queue.ScheduleAsync(original, DateTimeOffset.UtcNow);
        var started = DateTimeOffset.UtcNow;

        await worker.StartAsync(CancellationToken.None);
        try
        {
            var scheduled = await scheduler.Scheduled.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var continuation = Assert.IsType<CancelOpCancelPayment>(scheduled.Operation);
            Assert.Equal(original.PaymentUid, continuation.PaymentUid);
            Assert.True(scheduled.Due >= started.AddSeconds(10));
            Assert.Equal(2, processor.Calls);
        }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await worker.StopAsync(stop.Token);
        }
    }

    private sealed class TestProcessor : IDeferredOperationService
    {
        public int Calls;

        public Task<DeferredOperation?> ProcessAsync(
            DeferredOperation operation, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref Calls);
            if (operation is FinishOpReleaseCar)
                throw new AppConflictException("Terminal conflict");

            var cancel = (CancelOpCancelRental)operation;
            return Task.FromResult<DeferredOperation?>(new CancelOpCancelPayment(cancel.PaymentUid));
        }
    }

    private sealed class RecordingScheduler : IDeferredOperationScheduler
    {
        public TaskCompletionSource<(DeferredOperation Operation, DateTimeOffset Due)> Scheduled { get; }
            = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ScheduleAsync(DeferredOperation operation, DateTimeOffset executeAfter,
            CancellationToken cancellationToken = default)
        {
            Scheduled.TrySetResult((operation, executeAfter));
            return Task.CompletedTask;
        }
    }
}
