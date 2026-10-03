using Microsoft.EntityFrameworkCore;
using Rentals.Application.Dtos;
using Rentals.Application.Exceptions;
using Rentals.Application.OutPorts;
using Rentals.Storage.Context;
using Rentals.Storage.Mappers;
using Rentals.Domain.Models;

namespace Rentals.Storage.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RentalDto>> GetDtosByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _context.Rentals
            .AsNoTracking()
            .Where(r => r.Username == username)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);

        return rentals.ToDto();
    }

    public async Task<RentalDto?> GetDtoByUidAndUsernameAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RentalUid == rentalUid && r.Username == username, cancellationToken);

        return rental?.ToDto();
    }

    public async Task<Rental?> GetByUidAndUsernameAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals
            .FirstOrDefaultAsync(r => r.RentalUid == rentalUid && r.Username == username, cancellationToken);

        return rental?.ToDomain();
    }

    public async Task AddAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        await _context.Rentals.AddAsync(rental.ToDb(), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Rentals
            .FirstOrDefaultAsync(r => r.RentalUid == rental.RentalUid, cancellationToken)
            ?? throw new RentalNotFoundException(rental.RentalUid);
        existing.ApplyDomain(rental);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
