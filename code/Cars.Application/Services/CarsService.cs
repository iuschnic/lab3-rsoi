using Cars.Application.Dtos;
using Cars.Application.Exceptions;
using Cars.Application.InPorts;
using Cars.Application.Mappers;
using Cars.Application.OutPorts;
using Cars.Domain.Models;

namespace Cars.Application.Services;

public class CarsService : ICarsService
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    public const int MaxBatchSize = 100;

    private readonly ICarRepository _carRepository;

    public CarsService(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default)
    {
        if (page < MinPage)
            throw new AppValidationException("page", $"Page should be greater than or equal to {MinPage}");
        if (size < MinPageSize || size > MaxPageSize)
            throw new AppValidationException("size", $"Size should be between {MinPageSize} and {MaxPageSize}");

        return _carRepository.GetPageAsync(page, size, showAll, cancellationToken);
    }

    public async Task<CarDto> GetCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var car = await _carRepository.GetDtoByUidAsync(carUid, cancellationToken);

        return car ?? throw new CarNotFoundException(carUid);
    }

    public Task<IReadOnlyList<CarDto>> GetCarsAsync(
        IReadOnlyCollection<Guid> carUids,
        CancellationToken cancellationToken = default)
    {
        if (carUids.Count > MaxBatchSize)
            throw new AppValidationException("uids", $"Number of uids should not exceed {MaxBatchSize}");

        return _carRepository.GetDtosByUidsAsync(carUids, cancellationToken);
    }

    public async Task<CarDto> CreateCarAsync(
        CreateCarCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _carRepository.ExistsByRegistrationNumberAsync(
            command.RegistrationNumber, cancellationToken);
        if (exists)
            throw new CarAlreadyExistsException(command.RegistrationNumber);

        var car = Car.Create(
            command.Brand,
            command.Model,
            command.RegistrationNumber,
            command.Power,
            command.Price,
            command.Type.ToDomain());

        await _carRepository.AddAsync(car, cancellationToken);

        return car.ToDto();
    }

    public async Task ReserveCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var car = await GetDomainCarAsync(carUid, cancellationToken);
        car.Reserve();
        await _carRepository.UpdateAsync(car, cancellationToken);
    }

    public async Task ReleaseCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var car = await GetDomainCarAsync(carUid, cancellationToken);
        car.Release();
        await _carRepository.UpdateAsync(car, cancellationToken);
    }

    private async Task<Car> GetDomainCarAsync(Guid carUid, CancellationToken cancellationToken)
    {
        var car = await _carRepository.GetByUidAsync(carUid, cancellationToken);
        if (car is null)
            throw new CarNotFoundException(carUid);

        return car;
    }
}
