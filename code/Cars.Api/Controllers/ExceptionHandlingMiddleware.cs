using System.Text.Json;
using Cars.Api.Responses;
using Cars.Application.Exceptions;
using Cars.Domain.Exceptions;

namespace Cars.Api.Controllers;

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

            case DomainValidationException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                await response.WriteAsJsonAsync(new ValidationErrorResponse
                {
                    Message = "Invalid data",
                    Errors = new[]
                    {
                        new ErrorDescription { Field = "domain", Error = exception.Message }
                    }
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
            case CarUnavailableException:
                response.StatusCode = StatusCodes.Status409Conflict;
                await response.WriteAsJsonAsync(new ErrorResponse
                {
                    Message = exception.Message
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
}
