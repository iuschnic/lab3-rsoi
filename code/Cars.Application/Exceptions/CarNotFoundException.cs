namespace Cars.Application.Exceptions;

public class CarNotFoundException : AppNotFoundException
{
    public CarNotFoundException(Guid carUid) : base($"Car with uid {carUid} was not found") { }
}
