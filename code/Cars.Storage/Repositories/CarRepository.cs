using Cars.Application.Dtos;
using Cars.Application.Exceptions;
using Cars.Application.OutPorts;
using Cars.Domain.Models;
using Cars.Storage.Context;
using Cars.Storage.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Cars.Storage.Repositories;

public class CarRepository : ICarRepository
{
    private readonly AppDbContext _context;

    public CarRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<CarDto>> GetPageAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Cars.AsNoTracking();
        if (!showAll)
            query = query.Where(c => c.Availability);

        var totalElements = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CarDto>
        {
            Page = page,
            PageSize = size,
            TotalElements = totalElements,
            Items = items.ToDto(),
        };
    }

    public async Task<CarDto?> GetDtoByUidAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var car = await _context.Cars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CarUid == carUid, cancellationToken);

        return car?.ToDto();
    }

    public async Task<IReadOnlyList<CarDto>> GetDtosByUidsAsync(
        IReadOnlyCollection<Guid> carUids,
        CancellationToken cancellationToken = default)
    {
        var cars = await _context.Cars
            .AsNoTracking()
            .Where(c => carUids.Contains(c.CarUid))
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return cars.ToDto();
    }

    public async Task<Car?> GetByUidAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var car = await _context.Cars
            .FirstOrDefaultAsync(c => c.CarUid == carUid, cancellationToken);

        return car?.ToDomain();
    }

    public Task<bool> ExistsByRegistrationNumberAsync(
        string registrationNumber,
        CancellationToken cancellationToken = default)
    {
        return _context.Cars
            .AsNoTracking()
            .AnyAsync(c => c.RegistrationNumber == registrationNumber, cancellationToken);
    }

    public async Task AddAsync(Car car, CancellationToken cancellationToken = default)
    {
        await _context.Cars.AddAsync(car.ToDb(), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Cars
            .FirstOrDefaultAsync(c => c.CarUid == car.CarUid, cancellationToken) ?? throw new CarNotFoundException(car.CarUid);
        existing.ApplyDomain(car);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
