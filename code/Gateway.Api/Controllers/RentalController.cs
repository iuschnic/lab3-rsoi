using System.ComponentModel.DataAnnotations;
using Gateway.Api.Mappers;
using Gateway.Api.Requests;
using Gateway.Api.Responses;
using Gateway.Application.InPorts;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/v1/rental")]
public class RentalController : ControllerBase
{
    private const string UserNameHeader = "X-User-Name";

    private readonly ICarRentalService _carRentalService;

    public RentalController(ICarRentalService carRentalService)
    {
        _carRentalService = carRentalService;
    }

    /// <summary>
    /// Получить информацию о всех арендах пользователя.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RentalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RentalResponse>>> GetRentals(
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _carRentalService.GetUserRentalsAsync(username, cancellationToken);
        return Ok(rentals.ToResponse());
    }

    /// <summary>
    /// Информация по конкретной аренде пользователя.
    /// </summary>
    [HttpGet("{rentalUid:guid}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> GetRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _carRentalService.GetUserRentalAsync(rentalUid, username, cancellationToken);
        return Ok(rental.ToResponse());
    }

    /// <summary>
    /// Забронировать автомобиль.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateRentalResponse>> CreateRental(
        [FromHeader(Name = UserNameHeader), Required] string username,
        [FromBody] CreateRentalRequest request,
        CancellationToken cancellationToken = default)
    {
        var rental = await _carRentalService.CreateRentalAsync(
            request.CarUid,
            username,
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        return Ok(rental.ToCreateResponse());
    }

    /// <summary>
    /// Завершение аренды автомобиля.
    /// </summary>
    [HttpPost("{rentalUid:guid}/finish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinishRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        await _carRentalService.FinishRentalAsync(rentalUid, username, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Отмена аренды автомобиля.
    /// </summary>
    [HttpDelete("{rentalUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        await _carRentalService.CancelRentalAsync(rentalUid, username, cancellationToken);
        return NoContent();
    }
}
