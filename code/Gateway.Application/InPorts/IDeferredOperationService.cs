using Gateway.Application.Operations;

namespace Gateway.Application.InPorts;

public interface IDeferredOperationService
{
    Task ProcessAsync(
        CancelRentalDeferredOperation operation,
        CancellationToken cancellationToken = default);

    Task ProcessAsync(
        FinishRentalDeferredOperation operation,
        CancellationToken cancellationToken = default);
}