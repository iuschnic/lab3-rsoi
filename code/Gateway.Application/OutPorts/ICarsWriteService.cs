using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface ICarsWriteService
{
    Task ReserveCarAsync(Guid carUid, CancellationToken cancellationToken = default);

    Task ReleaseCarAsync(Guid carUid, CancellationToken cancellationToken = default);
}
