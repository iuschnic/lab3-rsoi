namespace Gateway.Application.Exceptions;

public class CriticalServiceUnavailableException : AppException
{
    public string ServiceName { get; }

    public CriticalServiceUnavailableException(ServiceUnavailableException innerException)
        : base(innerException.Message, innerException)
    {
        ServiceName = innerException.ServiceName;
    }
}
