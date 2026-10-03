namespace Cars.Api.Responses;

public class CarResponse
{
    public Guid CarUid { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public int Power { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Price { get; init; }
    public bool Available { get; init; }
}
