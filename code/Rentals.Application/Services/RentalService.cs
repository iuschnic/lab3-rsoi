using Rentals.Application.Dtos;
using Rentals.Application.Exceptions;
using Rentals.Application.InPorts;
using Rentals.Application.Mappers;
using Rentals.Application.OutPorts;
using Rentals.Domain.Models;

namespace Rentals.Application.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;

    public RentalService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public Task<IReadOnlyList<RentalDto>> GetUserRentalsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return _rentalRepository.GetDtosByUsernameAsync(username, cancellationToken);
    }

    public async Task<RentalDto> GetUserRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepository.GetDtoByUidAndUsernameAsync(rentalUid, username, cancellationToken);

        return rental ?? throw new RentalNotFoundException(rentalUid);
    }

    public async Task<RentalDto> CreateRentalAsync(
        CreateRentalCommand command,
        CancellationToken cancellationToken = default)
    {
        var rental = Rental.Create(
            command.Username,
            command.CarUid,
            command.DateFrom.ToUtcDateTime(),
            command.DateTo.ToUtcDateTime(),
            command.PaymentUid);

        await _rentalRepository.AddAsync(rental, cancellationToken);

        return rental.ToDto();
    }

    public async Task<RentalDto> FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await GetDomainRentalAsync(rentalUid, username, cancellationToken);
        rental.Finish();
        await _rentalRepository.UpdateAsync(rental, cancellationToken);

        return rental.ToDto();
    }

    public async Task<RentalDto> CancelRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await GetDomainRentalAsync(rentalUid, username, cancellationToken);
        rental.Cancel();
        await _rentalRepository.UpdateAsync(rental, cancellationToken);

        return rental.ToDto();
    }

    private async Task<Rental> GetDomainRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken)
    {
        var rental = await _rentalRepository.GetByUidAndUsernameAsync(rentalUid, username, cancellationToken);

        return rental ?? throw new RentalNotFoundException(rentalUid);
    }
}
