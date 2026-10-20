using Gateway.Application.Enums;

namespace Gateway.Application.Dtos;

public class RentalAggregate
{
    public Guid RentalUid { get; init; }
    public string Username { get; init; } = string.Empty;
    public RentalStatusDto Status { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public CarDto? Car { get; init; } = null!;
    public PaymentDto? Payment { get; init; } = null!;
}
