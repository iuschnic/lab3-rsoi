namespace Payments.Api.Responses;

public class PaymentResponse
{
    public Guid PaymentUid { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Price { get; init; }
}
