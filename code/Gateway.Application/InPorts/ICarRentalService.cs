using Gateway.Application.Dtos;

namespace Gateway.Application.InPorts;

public interface ICarRentalService
{
    Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RentalAggregate>> GetUserRentalsAsync(
        string username, 
        CancellationToken cancellationToken = default);

    Task<RentalAggregate> GetUserRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalAggregate> CreateRentalAsync(
        Guid carUid,
        string username, 
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default);

    Task CancelRentalAsync(
        Guid rentalUid, 
        string username, 
        CancellationToken cancellationToken = default);

    Task FinishRentalAsync(
        Guid rentalUid, 
        string username, 
        CancellationToken cancellationToken = default);
}
