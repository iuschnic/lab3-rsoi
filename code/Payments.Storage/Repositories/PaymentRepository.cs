using Microsoft.EntityFrameworkCore;
using Payments.Application.Dtos;
using Payments.Application.Exceptions;
using Payments.Application.OutPorts;
using Payments.Storage.Context;
using Payments.Storage.Mappers;
using Payments.Domain.Models;

namespace Payments.Storage.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> GetDtoByUidAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentUid == paymentUid, cancellationToken);

        return payment?.ToDto();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetDtosByUidsAsync(
        IReadOnlyCollection<Guid> paymentUids,
        CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => paymentUids.Contains(p.PaymentUid))
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return payments.ToDto();
    }

    public async Task<Payment?> GetByUidAsync(Guid paymentUid, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentUid == paymentUid, cancellationToken);

        return payment?.ToDomain();
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment.ToDb(), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentUid == payment.PaymentUid, cancellationToken)
            ?? throw new PaymentNotFoundException(payment.PaymentUid);
        existing.ApplyDomain(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
