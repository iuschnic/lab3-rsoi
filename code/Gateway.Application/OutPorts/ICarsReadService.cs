using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface ICarsReadService
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
}
