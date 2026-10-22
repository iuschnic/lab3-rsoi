using Gateway.Application.Dtos;

namespace Gateway.Application.OutPorts;

public interface IPaymentsReadService
{
    Task<PaymentDto?> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default);
}
