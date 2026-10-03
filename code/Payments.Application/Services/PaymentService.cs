using Payments.Application.Dtos;
using Payments.Application.Exceptions;
using Payments.Application.InPorts;
using Payments.Application.Mappers;
using Payments.Application.OutPorts;
using Payments.Domain.Models;

namespace Payments.Application.Services;

public class PaymentService : IPaymentService
{
    public const int MaxBatchSize = 100;

    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetDtoByUidAsync(paymentUid, cancellationToken);

        return payment ?? throw new PaymentNotFoundException(paymentUid);
    }

    public Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default)
    {
        if (paymentUids.Count > MaxBatchSize)
            throw new AppValidationException("uids", $"Number of uids should not exceed {MaxBatchSize}");

        return _paymentRepository.GetDtosByUidsAsync(paymentUids, cancellationToken);
    }

    public async Task<PaymentDto> CreatePaymentAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var payment = Payment.Create(command.Price);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        return payment.ToDto();
    }

    public async Task CancelPaymentAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByUidAsync(paymentUid, cancellationToken)
            ?? throw new PaymentNotFoundException(paymentUid);
        payment.Cancel();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
    }
}
