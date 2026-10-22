namespace Rentals.Domain.Exceptions;

public class RentalStatusTransitionException : DomainException
{
    public RentalStatusTransitionException(string message) : base(message) { }
    public RentalStatusTransitionException() : base("Invalid rental status transition") { }
}
