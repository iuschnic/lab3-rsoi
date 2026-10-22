using System.ComponentModel.DataAnnotations;
using Cars.Application.Enums;

namespace Cars.Api.Requests;

public class CreateCarRequest
{
    [Required]
    public string Brand { get; init; } = string.Empty;

    [Required]
    public string Model { get; init; } = string.Empty;

    [Required]
    public string RegistrationNumber { get; init; } = string.Empty;

    public int Power { get; init; }

    public int Price { get; init; }

    [Required]
    public CarTypeDto Type { get; init; }
}
