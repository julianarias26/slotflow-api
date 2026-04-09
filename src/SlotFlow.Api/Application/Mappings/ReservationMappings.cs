using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Application.Mappings;

public static class ReservationMappings
{
    public static ReservationDto ToDto(
        this Reservation reservation,
        Slot slot,
        Resource resource) =>
        new(
            reservation.Id,
            reservation.SlotId,
            slot.SlotNumber,
            resource.Id,
            resource.Name,
            reservation.UserId,
            reservation.Status.ToString(),
            reservation.HeldAt,
            reservation.ExpiresAt,
            reservation.ConfirmedAt,
            reservation.CancelledAt);
}