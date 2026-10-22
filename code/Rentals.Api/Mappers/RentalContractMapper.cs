using Rentals.Api.Requests;
using Rentals.Api.Responses;
using Rentals.Application.Dtos;
using Rentals.Application.Enums;

namespace Rentals.Api.Mappers;

public static class RentalContractMapper
{
    public static CreateRentalCommand ToCommand(this CreateRentalRequest request, string username) => new()
    {
        Username = username,
        CarUid = request.CarUid,
        PaymentUid = request.PaymentUid,
        DateFrom = request.DateFrom,
        DateTo = request.DateTo,
    };

    public static string ToResponse(this RentalStatusDto status) => status switch
    {
        RentalStatusDto.InProgress => "IN_PROGRESS",
        RentalStatusDto.Finished => "FINISHED",
        RentalStatusDto.Canceled => "CANCELED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalResponse ToResponse(this RentalDto dto) => new()
    {
        RentalUid = dto.RentalUid,
        Username = dto.Username,
        PaymentUid = dto.PaymentUid,
        CarUid = dto.CarUid,
        DateFrom = dto.DateFrom,
        DateTo = dto.DateTo,
        Status = dto.Status.ToResponse(),
    };

    public static IReadOnlyList<RentalResponse> ToResponse(this IEnumerable<RentalDto> dtos)
        => dtos.Select(d => d.ToResponse()).ToList();
}
