namespace Gateway.Api.Responses;

public class CreateRentalResponse
{
    public Guid RentalUid { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid CarUid { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public PaymentInfo Payment { get; init; } = null!;
}
