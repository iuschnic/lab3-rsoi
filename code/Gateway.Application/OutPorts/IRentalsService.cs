using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IRentalsService
{
    Task<IReadOnlyList<RentalDto>> GetRentalsAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto?> GetRentalAsync(
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
