namespace Rentals.Application.Exceptions;

public class RentalNotFoundException : AppNotFoundException
{
    public RentalNotFoundException(Guid rentalUid) : base($"Rental with uid {rentalUid} was not found") { }
}
