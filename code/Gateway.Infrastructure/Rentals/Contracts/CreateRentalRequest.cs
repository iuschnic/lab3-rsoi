namespace Gateway.Infrastructure.Rentals.Contracts;

public class CreateRentalRequest
{
    public Guid CarUid { get; init; }
    public Guid PaymentUid { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
}
