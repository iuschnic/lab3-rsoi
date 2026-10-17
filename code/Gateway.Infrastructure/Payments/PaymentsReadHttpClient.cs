using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;
using Gateway.Infrastructure.Payments.Contracts;

namespace Gateway.Infrastructure.Payments;

public class PaymentsReadHttpClient : HttpClientBase, IPaymentsReadService
{
    private const string BasePath = "api/v1/payments";

    public PaymentsReadHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Payments";

    public async Task<PaymentDto?> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}/{paymentUid}");

        var response = await SendOrDefaultOnNotFoundAsync<PaymentResponse>(request, cancellationToken);
        return response?.ToDto();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default)
    {
        if (paymentUids.Count == 0)
            return Array.Empty<PaymentDto>();

        var query = string.Join("&", paymentUids.Select(uid => $"uids={uid}"));
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BasePath}?{query}");

        var response = await SendAsync<List<PaymentResponse>>(request, cancellationToken);
        return response.ToDto();
    }
}
