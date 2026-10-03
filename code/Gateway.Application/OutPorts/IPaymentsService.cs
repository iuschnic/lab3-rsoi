using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IPaymentsService
{
    Task<PaymentDto?> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> CreatePaymentAsync(int price, CancellationToken cancellationToken = default);

    Task CancelPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);
}
