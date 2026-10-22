namespace Gateway.Infrastructure.Payments.Contracts;

public class PaymentResponse
{
    public Guid PaymentUid { get; init; }
    public PaymentStatusContract Status { get; init; }
    public int Price { get; init; }
}
