using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IRentalsWriteService
{
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
