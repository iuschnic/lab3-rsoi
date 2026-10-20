using Gateway.Application.Dtos;

namespace Gateway.Application.Mappers;

public static class RentalAggregateMapper
{
    public static RentalAggregate ToAggregate(this RentalDto rental, CarDto? car, PaymentDto? payment) => new()
    {
        RentalUid = rental.RentalUid,
        Username = rental.Username,
        Status = rental.Status,
        DateFrom = rental.DateFrom,
        DateTo = rental.DateTo,
        Car = car,
        Payment = payment,
    };
}