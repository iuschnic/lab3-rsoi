using System.Text.Json;
using Gateway.Api.Responses;
using Gateway.Application.Exceptions;

namespace Gateway.Api.Controllers;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception, "Response has already started, cannot write error body");
            return;
        }

        var response = context.Response;
        response.Clear();
        response.ContentType = "application/json";

        switch (exception)
        {
            case AppValidationException validationException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                await response.WriteAsJsonAsync(new ValidationErrorResponse
                {
                    Message = validationException.Message,
                    Errors = validationException.Errors
                        .Select(e => new ErrorDescription { Field = e.Key, Error = e.Value })
                        .ToList()
                }, JsonOptions);
                break;

            case AppNotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = exception.Message
                }, JsonOptions);
                break;

            case AppConflictException:
                response.StatusCode = StatusCodes.Status409Conflict;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = exception.Message
                }, JsonOptions);
                break;

            case AppInconsistentDataException:
                _logger.LogError(exception, "Inconsistent data between services on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = exception.Message
                }, JsonOptions);
                break;

            case CriticalServiceUnavailableException criticalException:
                _logger.LogError(exception, "Critical {Service} service call failed on {Method} {Path}",
                    criticalException.ServiceName,
                    context.Request.Method,
                    context.Request.Path);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = $"{GetPublicServiceName(criticalException.ServiceName)} Service unavailable"
                }, JsonOptions);
                break;

            case ServiceUnavailableException serviceUnavailableException:
                _logger.LogError(exception, "{Service} service call failed on {Method} {Path}",
                    serviceUnavailableException.ServiceName,
                    context.Request.Method,
                    context.Request.Path);
                response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = $"{GetPublicServiceName(serviceUnavailableException.ServiceName)} Service unavailable"
                }, JsonOptions);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = "An unexpected error occurred"
                }, JsonOptions);
                break;
        }
    }

    private static string GetPublicServiceName(string serviceName) => serviceName switch
    {
        "Payments" => "Payment",
        "Rentals" => "Rental",
        "Cars" => "Car",
        _ => serviceName
    };
}
