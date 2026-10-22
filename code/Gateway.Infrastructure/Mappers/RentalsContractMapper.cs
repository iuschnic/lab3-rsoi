using Gateway.Application.Dtos;
using Gateway.Application.Enums;
using Gateway.Infrastructure.Rentals.Contracts;

namespace Gateway.Infrastructure.Mappers;

public static class RentalsContractMapper
{
    public static RentalStatusDto ToDto(this RentalStatusContract status) => status switch
    {
        RentalStatusContract.InProgress => RentalStatusDto.InProgress,
        RentalStatusContract.Finished => RentalStatusDto.Finished,
        RentalStatusContract.Canceled => RentalStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalDto ToDto(this RentalResponse response) => new()
    {
        RentalUid = response.RentalUid,
        Username = response.Username,
        PaymentUid = response.PaymentUid,
        CarUid = response.CarUid,
        DateFrom = response.DateFrom,
        DateTo = response.DateTo,
        Status = response.Status.ToDto(),
    };

    public static IReadOnlyList<RentalDto> ToDto(this IEnumerable<RentalResponse> responses)
        => responses.Select(r => r.ToDto()).ToList();

    public static CreateRentalRequest ToRequest(this CreateRentalCommand command) => new()
    {
        CarUid = command.CarUid,
        PaymentUid = command.PaymentUid,
        DateFrom = command.DateFrom,
        DateTo = command.DateTo,
    };
}
