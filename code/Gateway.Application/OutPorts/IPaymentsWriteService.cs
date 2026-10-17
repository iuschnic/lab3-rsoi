using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IPaymentsWriteService
{
    Task<PaymentDto> CreatePaymentAsync(int price, CancellationToken cancellationToken = default);

    Task CancelPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);
}
