using Cars.Application.Dtos;
using Cars.Application.Enums;
using Cars.Domain.Enums;
using Cars.Domain.Models;

namespace Cars.Application.Mappers;

public static class CarMapper
{
    public static CarDto ToDto(this Car car) => new()
    {
        CarUid = car.CarUid,
        Brand = car.Brand,
        Model = car.Model,
        RegistrationNumber = car.RegistrationNumber,
        Power = car.Power,
        Price = car.Price,
        Type = car.Type.ToDto(),
        Availability = car.Availability,
    };

    public static IReadOnlyList<CarDto> ToDto(this IEnumerable<Car> cars)
        => cars.Select(c => c.ToDto()).ToList();

    public static CarTypeDto ToDto(this CarType type) => type switch
    {
        CarType.Sedan => CarTypeDto.Sedan,
        CarType.Suv => CarTypeDto.Suv,
        CarType.Minivan => CarTypeDto.Minivan,
        CarType.Roadster => CarTypeDto.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarType ToDomain(this CarTypeDto type) => type switch
    {
        CarTypeDto.Sedan => CarType.Sedan,
        CarTypeDto.Suv => CarType.Suv,
        CarTypeDto.Minivan => CarType.Minivan,
        CarTypeDto.Roadster => CarType.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };
}
