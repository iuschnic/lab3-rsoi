using Gateway.Api.Mappers;
using Gateway.Api.Responses;
using Gateway.Application.InPorts;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/v1/cars")]
public class CarsController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 100;

    private readonly ICarRentalService _carRentalService;

    public CarsController(ICarRentalService carRentalService)
    {
        _carRentalService = carRentalService;
    }

    /// <summary>
    /// Получить список всех доступных для бронирования автомобилей.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginationResponse>> GetCars(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int size = DefaultPageSize,
        [FromQuery] bool showAll = false,
        CancellationToken cancellationToken = default)
    {
        var cars = await _carRentalService.GetCarsAsync(page, size, showAll, cancellationToken);
        return Ok(cars.ToResponse());
    }
}
