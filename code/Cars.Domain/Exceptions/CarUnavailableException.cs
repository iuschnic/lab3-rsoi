namespace Cars.Domain.Exceptions;

public class CarUnavailableException : DomainException
{
    public CarUnavailableException(string message) : base(message) { }
    public CarUnavailableException() : base("Car is not available") { }
}
