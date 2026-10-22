using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IRentalsReadService
{
    Task<IReadOnlyList<RentalDto>> GetRentalsAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto?> GetRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);
}
