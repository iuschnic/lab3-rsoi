using Cars.Storage.Enums;

namespace Cars.Storage.Models;

public class CarDb
{
    public int Id { get; set; }
    public Guid CarUid { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int Power { get; set; }
    public int Price { get; set; }
    public CarTypeDb Type { get; set; }
    public bool Availability { get; set; }
}
