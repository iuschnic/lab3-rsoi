using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface ICarsService
{
    Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarDto>> GetCarsAsync(
        IReadOnlyCollection<Guid> carUids,
        CancellationToken cancellationToken = default);

    Task<CarDto?> GetCarAsync(Guid carUid, CancellationToken cancellationToken = default);

    Task ReserveCarAsync(Guid carUid, CancellationToken cancellationToken = default);

    Task ReleaseCarAsync(Guid carUid, CancellationToken cancellationToken = default);
}
