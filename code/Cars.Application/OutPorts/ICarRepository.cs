using Cars.Application.Dtos;
using Cars.Domain.Models;

namespace Cars.Application.OutPorts;

public interface ICarRepository
{
    Task<PagedResultDto<CarDto>> GetPageAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default);

    Task<CarDto?> GetDtoByUidAsync(Guid carUid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarDto>> GetDtosByUidsAsync(
        IReadOnlyCollection<Guid> carUids,
        CancellationToken cancellationToken = default);

    Task<Car?> GetByUidAsync(Guid carUid, CancellationToken cancellationToken = default);

    Task<bool> ExistsByRegistrationNumberAsync(
        string registrationNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(Car car, CancellationToken cancellationToken = default);

    Task UpdateAsync(Car car, CancellationToken cancellationToken = default);
}
