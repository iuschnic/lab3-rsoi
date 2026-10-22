namespace Cars.Api.Responses;

public class ValidationErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<ErrorDescription> Errors { get; init; } = Array.Empty<ErrorDescription>();
}

public class ErrorDescription
{
    public string Field { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}
