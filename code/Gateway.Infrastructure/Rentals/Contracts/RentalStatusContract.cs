using System.Text.Json.Serialization;

namespace Gateway.Infrastructure.Rentals.Contracts;

public enum RentalStatusContract
{
    [JsonStringEnumMemberName("IN_PROGRESS")]
    InProgress,

    [JsonStringEnumMemberName("FINISHED")]
    Finished,

    [JsonStringEnumMemberName("CANCELED")]
    Canceled
}
