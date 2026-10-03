using Rentals.Application.Dtos;

namespace Rentals.Application.InPorts;

public interface IRentalService
{
    Task<IReadOnlyList<RentalDto>> GetUserRentalsAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto> GetUserRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto> CreateRentalAsync(
        CreateRentalCommand command,
        CancellationToken cancellationToken = default);

    Task<RentalDto> FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto> CancelRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);
}
