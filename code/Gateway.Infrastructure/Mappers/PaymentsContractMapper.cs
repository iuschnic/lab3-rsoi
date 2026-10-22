using Gateway.Application.Dtos;
using Gateway.Application.Enums;
using Gateway.Infrastructure.Payments.Contracts;

namespace Gateway.Infrastructure.Mappers;

public static class PaymentsContractMapper
{
    public static PaymentStatusDto ToDto(this PaymentStatusContract status) => status switch
    {
        PaymentStatusContract.Paid => PaymentStatusDto.Paid,
        PaymentStatusContract.Canceled => PaymentStatusDto.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentDto ToDto(this PaymentResponse response) => new()
    {
        PaymentUid = response.PaymentUid,
        Status = response.Status.ToDto(),
        Price = response.Price,
    };

    public static IReadOnlyList<PaymentDto> ToDto(this IEnumerable<PaymentResponse> responses)
        => responses.Select(r => r.ToDto()).ToList();
}
