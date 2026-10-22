using Payments.Application.Dtos;

namespace Payments.Application.InPorts;

public interface IPaymentService
{
    Task<PaymentDto> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> CreatePaymentAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default);

    Task CancelPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default);
}
