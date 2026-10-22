namespace Cars.Application.Exceptions;

public class AppConflictException : AppException
{
    public AppConflictException(string message) : base(message) { }
    public AppConflictException() : base("Conflicting state") { }
}
