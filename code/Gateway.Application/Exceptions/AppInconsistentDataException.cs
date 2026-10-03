namespace Gateway.Application.Exceptions;

public class AppInconsistentDataException : AppException
{
    public AppInconsistentDataException(string message) : base(message) { }
    public AppInconsistentDataException() : base("Conflict in data consistence") { }
}
