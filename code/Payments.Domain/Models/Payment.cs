using Payments.Domain.Enums;
using Payments.Domain.Exceptions;

namespace Payments.Domain.Models;

public class Payment
{
    public Guid PaymentUid { get; private set; }
    public PaymentStatus Status { get; private set; }
    public int Price { get; private set; }

    private Payment(Guid paymentUid, PaymentStatus status, int price)
    {
        Validate(paymentUid, price);
        PaymentUid = paymentUid;
        Status = status;
        Price = price;
    }

    public static Payment Create(int price)
    {
        return new Payment(Guid.NewGuid(), PaymentStatus.Paid, price);
    }

    public static Payment Restore(Guid paymentUid, PaymentStatus status, int price)
    {
        return new Payment(paymentUid, status, price);
    }

    public void Cancel()
    {
        if (Status != PaymentStatus.Paid)
            throw new PaymentStatusTransitionException($"Payment {PaymentUid} cannot be canceled from status {Status}");
        Status = PaymentStatus.Canceled;
    }

    private static void Validate(Guid paymentUid, int price)
    {
        CheckUid(paymentUid);
        CheckPrice(price);
    }

    private static void CheckUid(Guid uuid)
    {
        if (uuid == Guid.Empty)
            throw new DomainValidationException($"Uuid should not be empty");
    }

    private static void CheckPrice(int price)
    {
        if (price <= 0)
            throw new DomainValidationException($"Price should be positive");
    }
}
