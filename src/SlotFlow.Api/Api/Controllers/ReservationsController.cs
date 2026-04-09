using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SlotFlow.Api.Api.Extensions;
using SlotFlow.Api.Api.Validators;
using SlotFlow.Api.Application.UseCases.Reservations;
using SlotFlow.Api.Domain.Enums;

namespace SlotFlow.Api.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController(
    HoldSlot holdSlot,
    ConfirmReservation confirmReservation,
    CancelReservation cancelReservation,
    GetReservationById getReservationById,
    GetUserReservations getUserReservations) : ControllerBase
{
    private const string UserIdHeader = "X-User-Id";

    private string? CurrentUserId =>
        Request.Headers.TryGetValue(UserIdHeader, out var value)
            ? value.ToString()
            : null;

    [HttpPost("hold")]
    public async Task<IActionResult> Hold(
        [FromBody] HoldSlotRequest request,
        [FromServices] IValidator<HoldSlotRequest> validator,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "Header X-User-Id is required."
            });

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "One or more validation errors occurred.",
                errors = validation.ToDictionary()
            });

        var result = await holdSlot.ExecuteAsync(
            new HoldSlot.Request(request.SlotId, userId), ct);

        return result.ToHttpResult(dto =>
            CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "Header X-User-Id is required."
            });

        var result = await confirmReservation.ExecuteAsync(
            new ConfirmReservation.Request(id, userId), ct);

        return result.ToHttpResult(Ok);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "Header X-User-Id is required."
            });

        var result = await cancelReservation.ExecuteAsync(
            new CancelReservation.Request(id, userId), ct);

        return result.ToHttpResult(Ok);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "Header X-User-Id is required."
            });

        var result = await getReservationById.ExecuteAsync(
            new GetReservationById.Request(id, userId), ct);

        return result.ToHttpResult(Ok);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserReservations(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                code = "Validation.Failed",
                message = "Header X-User-Id is required."
            });

        ReservationStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ReservationStatus>(status, ignoreCase: true, out var s))
            parsedStatus = s;

        var result = await getUserReservations.ExecuteAsync(userId, parsedStatus, ct);
        return Ok(result);
    }
}