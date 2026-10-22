namespace Payments.Application.Exceptions;

public class AppNotFoundException : AppException
{
    public AppNotFoundException(string message) : base(message) { }
    public AppNotFoundException() : base("Requested resource was not found") { }
}
