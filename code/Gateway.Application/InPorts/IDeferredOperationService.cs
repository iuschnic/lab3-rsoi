using Gateway.Application.Operations;

namespace Gateway.Application.InPorts;

public interface IDeferredOperationService
{
    Task ProcessAsync(
        DeferredOperation operation,
        CancellationToken cancellationToken = default);
}