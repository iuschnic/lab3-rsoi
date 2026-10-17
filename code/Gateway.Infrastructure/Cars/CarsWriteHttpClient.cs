using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Cars.Contracts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;

namespace Gateway.Infrastructure.Cars;

public class CarsWriteHttpClient : HttpClientBase, ICarsWriteService
{
    private const string BasePath = "api/v1/cars";

    public CarsWriteHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Cars";

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
