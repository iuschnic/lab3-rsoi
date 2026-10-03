namespace Gateway.Application.Exceptions;

/// <summary>
/// Сервис не ответил или ответил ошибкой, которую gateway обработать не может.
/// </summary>
public class ServiceUnavailableException : AppException
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName, string message)
        : base(message)
    {
        ServiceName = serviceName;
    }

    public ServiceUnavailableException(string serviceName, string message, Exception innerException)
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}
