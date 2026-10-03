using System.Text.Json.Serialization;

namespace Gateway.Infrastructure.Payments.Contracts;

public enum PaymentStatusContract
{
    [JsonStringEnumMemberName("PAID")]
    Paid,

    [JsonStringEnumMemberName("CANCELED")]
    Canceled
}
