namespace Gateway.Api.Responses;

public class PaymentInfo
{
    public Guid PaymentUid { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Price { get; init; }
}
