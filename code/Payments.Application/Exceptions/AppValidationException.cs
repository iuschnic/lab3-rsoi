namespace Payments.Application.Exceptions;

public class AppValidationException : AppException
{
    public IReadOnlyDictionary<string, string> Errors { get; }

    public AppValidationException(string field, string error)
        : base("Invalid data")
    {
        Errors = new Dictionary<string, string> { [field] = error };
    }

    public AppValidationException(IReadOnlyDictionary<string, string> errors)
        : base("Invalid data")
    {
        Errors = errors;
    }
}
