using System.Text.Json.Serialization;
using Gateway.Api.Serialization;

namespace Gateway.Api.Responses;

public class RentalResponse
{
    public Guid RentalUid { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public CarInfo? Car { get; init; } = null!;
    [JsonConverter(typeof(NullToEmptyObjectConverter<PaymentInfo>))]
    public PaymentInfo? Payment { get; init; }
}
