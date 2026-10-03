using Cars.Api.Mappers;
using Cars.Api.Requests;
using Cars.Api.Responses;
using Cars.Application.InPorts;
using Cars.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cars.Api.Controllers;

[ApiController]
[Route("api/v1/cars")]
public class CarsController : ControllerBase
{
    private readonly ICarsService _carsService;

    public CarsController(ICarsService carsService)
    {
        _carsService = carsService;
    }

    /// <summary>
    /// Получить список автомобилей. По умолчанию выводятся только доступные для бронирования.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginationResponse>> GetCars(
        [FromQuery] int page = CarsService.MinPage,
        [FromQuery] int size = CarsService.MaxPageSize,
        [FromQuery] bool showAll = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _carsService.GetCarsAsync(page, size, showAll, cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Получить список автомобилей по их uid.
    /// </summary>
    [HttpGet("batch")]
    [ProducesResponseType(typeof(IReadOnlyList<CarResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CarResponse>>> GetCarsByUids(
        [FromQuery] Guid[] uids,
        CancellationToken cancellationToken = default)
    {
        var cars = await _carsService.GetCarsAsync(uids, cancellationToken);
        return Ok(cars.ToResponse());
    }

    /// <summary>
    /// Получить информацию о конкретном автомобиле.
    /// </summary>
    [HttpGet("{carUid:guid}")]
    [ProducesResponseType(typeof(CarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarResponse>> GetCar(
        Guid carUid,
        CancellationToken cancellationToken = default)
    {
        var car = await _carsService.GetCarAsync(carUid, cancellationToken);
        return Ok(car.ToResponse());
    }

    /// <summary>
    /// Добавить автомобиль.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CarResponse>> CreateCar(
        [FromBody] CreateCarRequest request,
        CancellationToken cancellationToken = default)
    {
        var car = await _carsService.CreateCarAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(
            nameof(GetCar),
            new { carUid = car.CarUid },
            car.ToResponse());
    }

    /// <summary>
    /// Зарезервировать автомобиль (availability = false).
    /// </summary>
    [HttpPost("{carUid:guid}/reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveCar(
        Guid carUid,
        CancellationToken cancellationToken = default)
    {
        await _carsService.ReserveCarAsync(carUid, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Снять резерв с автомобиля (availability = true).
    /// </summary>
    [HttpPost("{carUid:guid}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseCar(
        Guid carUid,
        CancellationToken cancellationToken = default)
    {
        await _carsService.ReleaseCarAsync(carUid, cancellationToken);
        return NoContent();
    }
}
