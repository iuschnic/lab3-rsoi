using Cars.Application.Dtos;
using Cars.Storage.Enums;
using Cars.Application.Enums;
using Cars.Domain.Enums;
using Cars.Domain.Models;
using Cars.Storage.Models;

namespace Cars.Storage.Mappers;

public static class CarDbMapper
{
    public static CarTypeDb ToDb(this CarType type) => type switch
    {
        CarType.Sedan => CarTypeDb.Sedan,
        CarType.Suv => CarTypeDb.Suv,
        CarType.Minivan => CarTypeDb.Minivan,
        CarType.Roadster => CarTypeDb.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarType ToDomain(this CarTypeDb type) => type switch
    {
        CarTypeDb.Sedan => CarType.Sedan,
        CarTypeDb.Suv => CarType.Suv,
        CarTypeDb.Minivan => CarType.Minivan,
        CarTypeDb.Roadster => CarType.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarTypeDto ToDto(this CarTypeDb type) => type switch
    {
        CarTypeDb.Sedan => CarTypeDto.Sedan,
        CarTypeDb.Suv => CarTypeDto.Suv,
        CarTypeDb.Minivan => CarTypeDto.Minivan,
        CarTypeDb.Roadster => CarTypeDto.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarDb ToDb(this Car car) => new()
    {
        CarUid = car.CarUid,
        Brand = car.Brand,
        Model = car.Model,
        RegistrationNumber = car.RegistrationNumber,
        Power = car.Power,
        Price = car.Price,
        Type = car.Type.ToDb(),
        Availability = car.Availability,
    };

    public static Car ToDomain(this CarDb car)
        => Car.Restore(
            car.CarUid,
            car.Brand,
            car.Model,
            car.RegistrationNumber,
            car.Power,
            car.Price,
            car.Type.ToDomain(),
            car.Availability);

    public static CarDto ToDto(this CarDb car) => new()
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

    public static IReadOnlyList<CarDto> ToDto(this IEnumerable<CarDb> cars)
        => cars.Select(c => c.ToDto()).ToList();

    public static void ApplyDomain(this CarDb target, Car car)
    {
        target.Brand = car.Brand;
        target.Model = car.Model;
        target.RegistrationNumber = car.RegistrationNumber;
        target.Power = car.Power;
        target.Price = car.Price;
        target.Type = car.Type.ToDb();
        target.Availability = car.Availability;
    }
}
