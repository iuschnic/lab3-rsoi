namespace Gateway.Application.Dtos;

public class CreateRentalCommand
{
    public string Username { get; init; } = string.Empty;
    public Guid CarUid { get; init; }
    public Guid PaymentUid { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
}
