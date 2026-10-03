using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Cars.Contracts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;

namespace Gateway.Infrastructure.Cars;

public class CarsHttpClient : HttpClientBase, ICarsService
{
    private const string BasePath = "api/v1/cars";

    public CarsHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Cars";

    public async Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default)
    {
        var uri = $"{BasePath}?page={page}&size={size}&showAll={showAll.ToString().ToLowerInvariant()}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var response = await SendAsync<PaginationResponse>(request, cancellationToken);
        return response.ToDto();
    }

    public async Task<IReadOnlyList<CarDto>> GetCarsAsync(
        IReadOnlyCollection<Guid> carUids,
        CancellationToken cancellationToken = default)
    {
        if (carUids.Count == 0)
            return Array.Empty<CarDto>();

        var query = string.Join("&", carUids.Select(uid => $"uids={uid}"));
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/batch?{query}");

        var response = await SendAsync<List<CarResponse>>(request, cancellationToken);
        return response.ToDto();
    }

    public async Task<CarDto?> GetCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/{carUid}");

        var response = await SendOrDefaultOnNotFoundAsync<CarResponse>(request, cancellationToken);
        return response?.ToDto();
    }

    public Task ReserveCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BasePath}/{carUid}/reserve");

        return SendAsync(request, cancellationToken);
    }

    public Task ReleaseCarAsync(Guid carUid, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BasePath}/{carUid}/release");

        return SendAsync(request, cancellationToken);
    }
}
