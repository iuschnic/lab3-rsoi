using Payments.Storage.Enums;

namespace Payments.Storage.Models;

public class PaymentDb
{
    public int Id { get; set; }
    public Guid PaymentUid { get; set; }
    public PaymentStatusDb Status { get; set; }
    public int Price { get; set; }
}
