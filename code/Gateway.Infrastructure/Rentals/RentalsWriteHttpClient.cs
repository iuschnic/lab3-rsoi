using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;
using Gateway.Infrastructure.Rentals.Contracts;

namespace Gateway.Infrastructure.Rentals;

public class RentalsWriteHttpClient : HttpClientBase, IRentalsWriteService
{
    private const string BasePath = "api/v1/rentals";
    private const string UserNameHeader = "X-User-Name";

    public RentalsWriteHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Rentals";

    public async Task<RentalDto> CreateRentalAsync(
        CreateRentalCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, BasePath, command.Username);
        request.Content = ToJsonContent(command.ToRequest());

        var response = await SendAsync<RentalResponse>(request, cancellationToken);
        return response.ToDto();
    }

    public async Task<RentalDto> FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, $"{BasePath}/{rentalUid}/finish", username);

        var response = await SendAsync<RentalResponse>(request, cancellationToken);
        return response.ToDto();
    }

    public async Task<RentalDto> CancelRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Delete, $"{BasePath}/{rentalUid}", username);

        var response = await SendAsync<RentalResponse>(request, cancellationToken);
        return response.ToDto();
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, string username)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(UserNameHeader, username);
        return request;
    }
}
