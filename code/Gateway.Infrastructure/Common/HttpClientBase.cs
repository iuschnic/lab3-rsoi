using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gateway.Application.Exceptions;

namespace Gateway.Infrastructure.Common;

public abstract class HttpClientBase
{
    protected static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    protected readonly HttpClient HttpClient;

    protected HttpClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected abstract string ServiceName { get; }

    protected async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadAsync<T>(response, cancellationToken);
    }

    protected async Task<T?> SendOrDefaultOnNotFoundAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        where T : class
    {
        using var response = await SendCoreAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken);

        return await ReadAsync<T>(response, cancellationToken);
    }

    protected async Task SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected static JsonContent ToJsonContent<T>(T body) => JsonContent.Create(body, options: JsonOptions);

    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await HttpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw Unavailable($"{ServiceName} service is unreachable", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw Unavailable($"{ServiceName} service request timed out", exception);
        }
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        T? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (JsonException exception)
        {
            throw Unavailable($"{ServiceName} service returned malformed response body", exception);
        }

        return result ?? throw Unavailable($"{ServiceName} service returned empty response body");
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var details = string.IsNullOrWhiteSpace(body) ? "no details" : body;

        throw response.StatusCode switch
        {
            HttpStatusCode.NotFound
                => new AppNotFoundException($"{ServiceName} service: resource was not found"),
            HttpStatusCode.Conflict
                => new AppConflictException($"{ServiceName} service: {details}"),
            HttpStatusCode.BadRequest
                => new AppValidationException(ServiceName.ToLowerInvariant(), details),
            _ => Unavailable(
                $"{ServiceName} service responded with {(int)response.StatusCode} {response.StatusCode}: {details}"),
        };
    }

    private ServiceUnavailableException Unavailable(string message)
        => new(ServiceName, message);

    private ServiceUnavailableException Unavailable(string message, Exception innerException)
        => new(ServiceName, message, innerException);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
