using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Rentals.Api.Mappers;
using Rentals.Api.Requests;
using Rentals.Api.Responses;
using Rentals.Application.InPorts;

namespace Rentals.Api.Controllers;

[ApiController]
[Route("api/v1/rentals")]
public class RentalController : ControllerBase
{
    private const string UserNameHeader = "X-User-Name";

    private readonly IRentalService _rentalService;

    public RentalController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    /// <summary>
    /// Получить все аренды пользователя.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RentalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RentalResponse>>> GetRentals(
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalService.GetUserRentalsAsync(username, cancellationToken);
        return Ok(rentals.ToResponse());
    }

    /// <summary>
    /// Получить аренду пользователя по uid.
    /// </summary>
    [HttpGet("{rentalUid:guid}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalResponse>> GetRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalService.GetUserRentalAsync(rentalUid, username, cancellationToken);
        return Ok(rental.ToResponse());
    }

    /// <summary>
    /// Создать аренду (статус IN_PROGRESS).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RentalResponse>> CreateRental(
        [FromHeader(Name = UserNameHeader), Required] string username,
        [FromBody] CreateRentalRequest request,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalService.CreateRentalAsync(request.ToCommand(username), cancellationToken);
        return CreatedAtAction(
            nameof(GetRental),
            new { rentalUid = rental.RentalUid },
            rental.ToResponse());
    }

    /// <summary>
    /// Завершить аренду (статус FINISHED).
    /// </summary>
    [HttpPost("{rentalUid:guid}/finish")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RentalResponse>> FinishRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalService.FinishRentalAsync(rentalUid, username, cancellationToken);
        return Ok(rental.ToResponse());
    }

    /// <summary>
    /// Отменить аренду (статус CANCELED).
    /// </summary>
    [HttpDelete("{rentalUid:guid}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RentalResponse>> CancelRental(
        Guid rentalUid,
        [FromHeader(Name = UserNameHeader), Required] string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalService.CancelRentalAsync(rentalUid, username, cancellationToken);
        return Ok(rental.ToResponse());
    }
}
