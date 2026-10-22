using Gateway.Application.Dtos;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Common;
using Gateway.Infrastructure.Mappers;
using Gateway.Infrastructure.Payments.Contracts;

namespace Gateway.Infrastructure.Payments;

public class PaymentsWriteHttpClient : HttpClientBase, IPaymentsWriteService
{
    private const string BasePath = "api/v1/payments";

    public PaymentsWriteHttpClient(HttpClient httpClient) : base(httpClient) { }

    protected override string ServiceName => "Payments";

    public async Task<PaymentDto> CreatePaymentAsync(int price, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BasePath)
        {
            Content = ToJsonContent(new CreatePaymentRequest { Price = price }),
        };

        var response = await SendAsync<PaymentResponse>(request, cancellationToken);
        return response.ToDto();
    }

    public Task CancelPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BasePath}/{paymentUid}");

        return SendAsync(request, cancellationToken);
    }
}
