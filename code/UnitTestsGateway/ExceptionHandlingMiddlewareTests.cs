using System.Text.Json;
using Gateway.Api.Controllers;
using Gateway.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTestsGateway;

public class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData("Payments", "Payment", "Payments service is unreachable", false)]
    [InlineData("Payments", "Payment", "Payments service responded with 500 InternalServerError: database failed", false)]
    [InlineData("Rentals", "Rental", "Rentals service request timed out", false)]
    [InlineData("Cars", "Car", "Cars service is unavailable (circuit is open)", false)]
    [InlineData("Cars", "Car", "Cars service request timed out", true)]
    [InlineData("Rentals", "Rental", "Rentals service is unreachable", true)]
    public async Task UnavailableResponseUsesPublicServiceNameInsteadOfTechnicalDetails(
        string serviceName, string publicName, string details, bool critical)
    {
        var exception = new ServiceUnavailableException(serviceName, details,
            new HttpRequestException("Connection refused"));
        Exception failure = critical ? new CriticalServiceUnavailableException(exception) : exception;
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw failure,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        using var services = new ServiceCollection().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await middleware.InvokeAsync(context);

        Assert.Equal(critical ? StatusCodes.Status500InternalServerError : StatusCodes.Status503ServiceUnavailable,
            context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body);
        Assert.Equal($"{publicName} Service unavailable",
            json.RootElement.GetProperty("message").GetString());
        Assert.Single(json.RootElement.EnumerateObject());
    }
}
