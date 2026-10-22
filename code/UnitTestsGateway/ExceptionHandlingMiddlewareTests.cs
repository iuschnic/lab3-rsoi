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
    [InlineData("Payments", "Payment", "Payments service is unreachable")]
    [InlineData("Payments", "Payment", "Payments service responded with 500 InternalServerError: database failed")]
    [InlineData("Rentals", "Rental", "Rentals service request timed out")]
    [InlineData("Cars", "Car", "Cars service is unavailable (circuit is open)")]
    public async Task UnavailableResponseUsesPublicServiceNameInsteadOfTechnicalDetails(
        string serviceName, string publicName, string details)
    {
        var exception = new ServiceUnavailableException(serviceName, details,
            new HttpRequestException("Connection refused"));
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        using var services = new ServiceCollection().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        await using var body = new MemoryStream();
        context.Response.Body = body;

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        body.Position = 0;
        using var json = await JsonDocument.ParseAsync(body);
        Assert.Equal($"{publicName} Service unavailable",
            json.RootElement.GetProperty("message").GetString());
        Assert.Single(json.RootElement.EnumerateObject());
    }
}
