using Gateway.Application.Dtos;
using Gateway.Application.Enums;
using Gateway.Infrastructure.Cars.Contracts;

namespace Gateway.Infrastructure.Mappers;

public static class CarsContractMapper
{
    public static CarTypeDto ToDto(this CarTypeContract type) => type switch
    {
        CarTypeContract.Sedan => CarTypeDto.Sedan,
        CarTypeContract.Suv => CarTypeDto.Suv,
        CarTypeContract.Minivan => CarTypeDto.Minivan,
        CarTypeContract.Roadster => CarTypeDto.Roadster,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarDto ToDto(this CarResponse response) => new()
    {
        CarUid = response.CarUid,
        Brand = response.Brand,
        Model = response.Model,
        RegistrationNumber = response.RegistrationNumber,
        Power = response.Power,
        Price = response.Price,
        Type = response.Type.ToDto(),
        Availability = response.Available,
    };

    public static IReadOnlyList<CarDto> ToDto(this IEnumerable<CarResponse> responses)
        => responses.Select(r => r.ToDto()).ToList();

    public static PagedResultDto<CarDto> ToDto(this PaginationResponse response) => new()
    {
        Page = response.Page,
        PageSize = response.PageSize,
        TotalElements = response.TotalElements,
        Items = response.Items.ToDto(),
    };
}
