namespace Rentals.Api.Responses;

public class RentalResponse
{
    public Guid RentalUid { get; init; }
    public string Username { get; init; } = string.Empty;
    public Guid PaymentUid { get; init; }
    public Guid CarUid { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public string Status { get; init; } = string.Empty;
}
