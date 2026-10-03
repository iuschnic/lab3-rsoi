using Microsoft.AspNetCore.Mvc;
using Payments.Api.Mappers;
using Payments.Api.Requests;
using Payments.Api.Responses;
using Payments.Application.InPorts;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Получить список платежей по их uid.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetPayments(
        [FromQuery] Guid[] uids,
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentService.GetPaymentsAsync(uids, cancellationToken);
        return Ok(payments.ToResponse());
    }

    /// <summary>
    /// Получить информацию о платеже.
    /// </summary>
    [HttpGet("{paymentUid:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> GetPayment(
        Guid paymentUid,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentService.GetPaymentAsync(paymentUid, cancellationToken);
        return Ok(payment.ToResponse());
    }

    /// <summary>
    /// Создать платеж (статус PAID).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentService.CreatePaymentAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(
            nameof(GetPayment),
            new { paymentUid = payment.PaymentUid },
            payment.ToResponse());
    }

    /// <summary>
    /// Отменить платеж (статус CANCELED).
    /// </summary>
    [HttpDelete("{paymentUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelPayment(
        Guid paymentUid,
        CancellationToken cancellationToken = default)
    {
        await _paymentService.CancelPaymentAsync(paymentUid, cancellationToken);
        return NoContent();
    }
}
