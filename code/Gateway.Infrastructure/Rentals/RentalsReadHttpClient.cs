using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;
using Gateway.Infrastructure.Rentals.Contracts;

namespace Gateway.Infrastructure.Rentals;

public class RentalsReadHttpClient : HttpClientBase, IRentalsReadService
{
    private const string BasePath = "api/v1/rentals";
    private const string UserNameHeader = "X-User-Name";

    public RentalsReadHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Rentals";

    public async Task<IReadOnlyList<RentalDto>> GetRentalsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, BasePath, username);

        var response = await SendAsync<List<RentalResponse>>(request, cancellationToken);
        return response.ToDto();
    }

    public async Task<RentalDto?> GetRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, $"{BasePath}/{rentalUid}", username);

        var response = await SendOrDefaultOnNotFoundAsync<RentalResponse>(request, cancellationToken);
        return response?.ToDto();
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, string username)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(UserNameHeader, username);
        return request;
    }
}
