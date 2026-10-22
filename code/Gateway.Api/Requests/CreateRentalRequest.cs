namespace Gateway.Api.Requests;

public class CreateRentalRequest
{
    public Guid CarUid { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
}
