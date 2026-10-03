using Gateway.Api.Responses;
using Gateway.Application.Dtos;
using Gateway.Application.Enums;

namespace Gateway.Api.Mappers;

public static class RentalContractMapper
{
    public static string ToResponse(this RentalStatusDto status) => status switch
    {
        RentalStatusDto.InProgress => "IN_PROGRESS",
        RentalStatusDto.Finished => "FINISHED",
        RentalStatusDto.Canceled => "CANCELED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    public static string ToResponse(this PaymentStatusDto status) => status switch
    {
        PaymentStatusDto.Paid => "PAID",
        PaymentStatusDto.Canceled => "CANCELED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentInfo ToInfo(this PaymentDto dto) => new()
    {
        PaymentUid = dto.PaymentUid,
        Status = dto.Status.ToResponse(),
        Price = dto.Price,
    };

    public static RentalResponse ToResponse(this RentalAggregate aggregate) => new()
    {
        RentalUid = aggregate.RentalUid,
        Status = aggregate.Status.ToResponse(),
        DateFrom = aggregate.DateFrom,
        DateTo = aggregate.DateTo,
        Car = aggregate.Car.ToInfo(),
        Payment = aggregate.Payment.ToInfo(),
    };

    public static IReadOnlyList<RentalResponse> ToResponse(this IEnumerable<RentalAggregate> aggregates)
        => aggregates.Select(a => a.ToResponse()).ToList();

    public static CreateRentalResponse ToCreateResponse(this RentalAggregate aggregate) => new()
    {
        RentalUid = aggregate.RentalUid,
        Status = aggregate.Status.ToResponse(),
        CarUid = aggregate.Car.CarUid,
        DateFrom = aggregate.DateFrom,
        DateTo = aggregate.DateTo,
        Payment = aggregate.Payment.ToInfo(),
    };
}
