using Payments.Application.Dtos;
using Payments.Application.Enums;
using Payments.Domain.Enums;
using Payments.Storage.Enums;
using Payments.Storage.Models;
using Payments.Domain.Models;

namespace Payments.Storage.Mappers;

public static class PaymentDbMapper
{
    public static PaymentStatusDb ToDb(this PaymentStatus status) => status switch
    {
        PaymentStatus.Paid => PaymentStatusDb.Paid,
        PaymentStatus.Canceled => PaymentStatusDb.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentStatus ToDomain(this PaymentStatusDb status) => status switch
    {
        PaymentStatusDb.Paid => PaymentStatus.Paid,
        PaymentStatusDb.Canceled => PaymentStatus.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentStatusDto ToDto(this PaymentStatusDb status) => status switch
    {
        PaymentStatusDb.Paid => PaymentStatusDto.Paid,
        PaymentStatusDb.Canceled => PaymentStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentDb ToDb(this Payment payment) => new()
    {
        PaymentUid = payment.PaymentUid,
        Status = payment.Status.ToDb(),
        Price = payment.Price,
    };

    public static Payment ToDomain(this PaymentDb payment)
        => Payment.Restore(
            payment.PaymentUid,
            payment.Status.ToDomain(),
            payment.Price);

    public static PaymentDto ToDto(this PaymentDb payment) => new()
    {
        PaymentUid = payment.PaymentUid,
        Status = payment.Status.ToDto(),
        Price = payment.Price,
    };

    public static IReadOnlyList<PaymentDto> ToDto(this IEnumerable<PaymentDb> payments)
        => payments.Select(p => p.ToDto()).ToList();

    public static void ApplyDomain(this PaymentDb target, Payment payment)
    {
        target.Status = payment.Status.ToDb();
        target.Price = payment.Price;
    }
}
