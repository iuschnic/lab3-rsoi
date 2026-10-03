namespace Gateway.Api.Responses;

public class CarInfo
{
    public Guid CarUid { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
}
