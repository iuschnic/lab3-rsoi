using Payments.Application.Dtos;
using Payments.Application.Enums;
using Payments.Domain.Enums;
using Payments.Domain.Models;

namespace Payments.Application.Mappers;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment payment) => new()
    {
        PaymentUid = payment.PaymentUid,
        Status = payment.Status.ToDto(),
        Price = payment.Price,
    };

    public static IReadOnlyList<PaymentDto> ToDto(this IEnumerable<Payment> payments)
        => payments.Select(p => p.ToDto()).ToList();

    public static PaymentStatusDto ToDto(this PaymentStatus status) => status switch
    {
        PaymentStatus.Paid => PaymentStatusDto.Paid,
        PaymentStatus.Canceled => PaymentStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentStatus ToDomain(this PaymentStatusDto status) => status switch
    {
        PaymentStatusDto.Paid => PaymentStatus.Paid,
        PaymentStatusDto.Canceled => PaymentStatus.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };
}
