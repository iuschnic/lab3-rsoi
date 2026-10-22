using Payments.Api.Requests;
using Payments.Api.Responses;
using Payments.Application.Dtos;
using Payments.Application.Enums;

namespace Payments.Api.Mappers;

public static class PaymentContractMapper
{
    public static CreatePaymentCommand ToCommand(this CreatePaymentRequest request) => new()
    {
        Price = request.Price,
    };

    public static string ToResponse(this PaymentStatusDto status) => status switch
    {
        PaymentStatusDto.Paid => "PAID",
        PaymentStatusDto.Canceled => "CANCELED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown payment status"),
    };

    public static PaymentResponse ToResponse(this PaymentDto dto) => new()
    {
        PaymentUid = dto.PaymentUid,
        Status = dto.Status.ToResponse(),
        Price = dto.Price,
    };

    public static IReadOnlyList<PaymentResponse> ToResponse(this IEnumerable<PaymentDto> dtos)
        => dtos.Select(d => d.ToResponse()).ToList();
}
