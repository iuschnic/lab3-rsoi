using Rentals.Application.Dtos;
using Rentals.Application.Enums;
using Rentals.Application.Mappers;
using Rentals.Domain.Enums;
using Rentals.Storage.Enums;
using Rentals.Storage.Models;
using Rentals.Domain.Models;

namespace Rentals.Storage.Mappers;

public static class RentalDbMapper
{
    public static RentalStatusDb ToDb(this RentalStatus status) => status switch
    {
        RentalStatus.InProgress => RentalStatusDb.InProgress,
        RentalStatus.Finished => RentalStatusDb.Finished,
        RentalStatus.Canceled => RentalStatusDb.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalStatus ToDomain(this RentalStatusDb status) => status switch
    {
        RentalStatusDb.InProgress => RentalStatus.InProgress,
        RentalStatusDb.Finished => RentalStatus.Finished,
        RentalStatusDb.Canceled => RentalStatus.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalStatusDto ToDto(this RentalStatusDb status) => status switch
    {
        RentalStatusDb.InProgress => RentalStatusDto.InProgress,
        RentalStatusDb.Finished => RentalStatusDto.Finished,
        RentalStatusDb.Canceled => RentalStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static RentalDb ToDb(this Rental rental) => new()
    {
        RentalUid = rental.RentalUid,
        Username = rental.Username,
        PaymentUid = rental.PaymentUid,
        CarUid = rental.CarUid,
        DateFrom = rental.DateFrom,
        DateTo = rental.DateTo,
        Status = rental.Status.ToDb(),
    };

    public static Rental ToDomain(this RentalDb rental)
        => Rental.Restore(
            rental.RentalUid,
            rental.Username,
            rental.PaymentUid,
            rental.CarUid,
            rental.DateFrom,
            rental.DateTo,
            rental.Status.ToDomain());

    public static RentalDto ToDto(this RentalDb rental) => new()
    {
        RentalUid = rental.RentalUid,
        Username = rental.Username,
        PaymentUid = rental.PaymentUid,
        CarUid = rental.CarUid,
        DateFrom = rental.DateFrom.ToDateOnly(),
        DateTo = rental.DateTo.ToDateOnly(),
        Status = rental.Status.ToDto(),
    };

    public static IReadOnlyList<RentalDto> ToDto(this IEnumerable<RentalDb> rentals)
        => rentals.Select(r => r.ToDto()).ToList();

    public static void ApplyDomain(this RentalDb target, Rental rental)
    {
        target.Username = rental.Username;
        target.PaymentUid = rental.PaymentUid;
        target.CarUid = rental.CarUid;
        target.DateFrom = rental.DateFrom;
        target.DateTo = rental.DateTo;
        target.Status = rental.Status.ToDb();
    }
}
