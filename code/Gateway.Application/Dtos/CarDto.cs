using Gateway.Application.Enums;

namespace Gateway.Application.Dtos;

public class CarDto
{
    public Guid CarUid { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public int Power { get; init; }
    public int Price { get; init; }
    public CarTypeDto Type { get; init; }
    public bool Availability { get; init; }
}
