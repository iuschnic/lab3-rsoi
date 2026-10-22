using Payments.Application.Enums;

namespace Payments.Application.Dtos;

public class PaymentDto
{
    public Guid PaymentUid { get; init; }
    public PaymentStatusDto Status { get; init; }
    public int Price { get; init; }
}
