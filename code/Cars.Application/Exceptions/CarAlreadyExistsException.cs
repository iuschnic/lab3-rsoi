namespace Cars.Application.Exceptions;

public class CarAlreadyExistsException : AppConflictException
{
    public CarAlreadyExistsException(string registrationNumber)
        : base($"Car with registration number {registrationNumber} already exists") { }
}
