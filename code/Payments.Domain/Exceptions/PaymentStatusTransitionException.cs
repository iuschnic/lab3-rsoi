namespace Payments.Domain.Exceptions;

public class PaymentStatusTransitionException : DomainException
{
    public PaymentStatusTransitionException(string message) : base(message) { }
    public PaymentStatusTransitionException() : base("Invalid payment status transition") { }
}
