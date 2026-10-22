using Cars.Application.Enums;

namespace Cars.Application.Dtos;

public class CreateCarCommand
{
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public int Power { get; init; }
    public int Price { get; init; }
    public CarTypeDto Type { get; init; }
}
