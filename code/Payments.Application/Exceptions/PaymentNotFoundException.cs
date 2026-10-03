namespace Payments.Application.Exceptions;

public class PaymentNotFoundException : AppNotFoundException
{
    public PaymentNotFoundException(Guid paymentUid) : base($"Payment with uid {paymentUid} was not found") { }
}
