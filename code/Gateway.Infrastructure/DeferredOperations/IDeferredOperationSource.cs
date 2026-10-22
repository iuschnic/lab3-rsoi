using Gateway.Application.Operations;

namespace Gateway.Infrastructure.DeferredOperations;

public interface IDeferredOperationSource
{
    ValueTask<DeferredOperation> ReadAsync(CancellationToken cancellationToken);
}
