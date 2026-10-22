using Payments.Application.Dtos;
using Payments.Domain.Models;

namespace Payments.Application.OutPorts;

public interface IPaymentRepository
{
    Task<PaymentDto?> GetDtoByUidAsync(Guid paymentUid, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetDtosByUidsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByUidAsync(Guid paymentUid, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
