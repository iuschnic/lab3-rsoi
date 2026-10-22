using System.Text.Json.Serialization;

namespace Gateway.Infrastructure.Cars.Contracts;

public enum CarTypeContract
{
    [JsonStringEnumMemberName("SEDAN")]
    Sedan,

    [JsonStringEnumMemberName("SUV")]
    Suv,

    [JsonStringEnumMemberName("MINIVAN")]
    Minivan,

    [JsonStringEnumMemberName("ROADSTER")]
    Roadster
}
