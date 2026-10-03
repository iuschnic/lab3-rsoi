using Rentals.Application.Dtos;
using Rentals.Domain.Models;

namespace Rentals.Application.OutPorts;

public interface IRentalRepository
{
    Task<IReadOnlyList<RentalDto>> GetDtosByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<RentalDto?> GetDtoByUidAndUsernameAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);

    Task<Rental?> GetByUidAndUsernameAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default);

    Task AddAsync(Rental rental, CancellationToken cancellationToken = default);

    Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
}
