using Gateway.Api.Responses;
using Gateway.Application.Dtos;
using Gateway.Application.Enums;

namespace Gateway.Api.Mappers;

public static class CarContractMapper
{
    public static string ToResponse(this CarTypeDto type) => type switch
    {
        CarTypeDto.Sedan => "SEDAN",
        CarTypeDto.Suv => "SUV",
        CarTypeDto.Minivan => "MINIVAN",
        CarTypeDto.Roadster => "ROADSTER",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown car type"),
    };

    public static CarResponse ToResponse(this CarDto dto) => new()
    {
        CarUid = dto.CarUid,
        Brand = dto.Brand,
        Model = dto.Model,
        RegistrationNumber = dto.RegistrationNumber,
        Power = dto.Power,
        Type = dto.Type.ToResponse(),
        Price = dto.Price,
        Available = dto.Availability,
    };

    public static IReadOnlyList<CarResponse> ToResponse(this IEnumerable<CarDto> dtos)
        => dtos.Select(d => d.ToResponse()).ToList();

    public static PaginationResponse ToResponse(this PagedResultDto<CarDto> page) => new()
    {
        Page = page.Page,
        PageSize = page.PageSize,
        TotalElements = page.TotalElements,
        Items = page.Items.ToResponse(),
    };

    public static CarInfo ToInfo(this CarDto dto) => new()
    {
        CarUid = dto.CarUid,
        Brand = dto.Brand,
        Model = dto.Model,
        RegistrationNumber = dto.RegistrationNumber,
    };
}
