using Rentals.Application.Dtos;
using Rentals.Application.Enums;
using Rentals.Domain.Enums;
using Rentals.Domain.Models;

namespace Rentals.Application.Mappers;

public static class RentalMapper
{
    public static RentalDto ToDto(this Rental rental) => new()
    {
        RentalUid = rental.RentalUid,
        Username = rental.Username,
        PaymentUid = rental.PaymentUid,
        CarUid = rental.CarUid,
        DateFrom = rental.DateFrom.ToDateOnly(),
        DateTo = rental.DateTo.ToDateOnly(),
        Status = rental.Status.ToDto(),
    };

    public static IReadOnlyList<RentalDto> ToDto(this IEnumerable<Rental> rentals)
        => rentals.Select(r => r.ToDto()).ToList();

    public static RentalStatusDto ToDto(this RentalStatus status) => status switch
    {
        RentalStatus.InProgress => RentalStatusDto.InProgress,
        RentalStatus.Finished => RentalStatusDto.Finished,
        RentalStatus.Canceled => RentalStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalStatus ToDomain(this RentalStatusDto status) => status switch
    {
        RentalStatusDto.InProgress => RentalStatus.InProgress,
        RentalStatusDto.Finished => RentalStatus.Finished,
        RentalStatusDto.Canceled => RentalStatus.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static DateTime ToUtcDateTime(this DateOnly date)
        => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    public static DateOnly ToDateOnly(this DateTime dateTime)
        => DateOnly.FromDateTime(dateTime.ToUniversalTime());
}
