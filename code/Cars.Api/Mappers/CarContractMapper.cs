using Cars.Api.Requests;
using Cars.Api.Responses;
using Cars.Application.Dtos;

namespace Cars.Api.Mappers;

public static class CarContractMapper
{
    public static CreateCarCommand ToCommand(this CreateCarRequest request) => new()
    {
        Brand = request.Brand,
        Model = request.Model,
        RegistrationNumber = request.RegistrationNumber,
        Power = request.Power,
        Price = request.Price,
        Type = request.Type,
    };

    public static CarResponse ToResponse(this CarDto dto) => new()
    {
        CarUid = dto.CarUid,
        Brand = dto.Brand,
        Model = dto.Model,
        RegistrationNumber = dto.RegistrationNumber,
        Power = dto.Power,
        Type = dto.Type.ToString().ToUpperInvariant(),
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
}
