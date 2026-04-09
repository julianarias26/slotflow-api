using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Domain.Enums;

namespace SlotFlow.Api.Application.UseCases.Reservations;

public sealed class GetUserReservations(
    IReservationRepository reservations,
    ISlotRepository slots,
    IResourceRepository resources)
{
    public async Task<List<UserReservationDto>> ExecuteAsync(
        string userId,
        ReservationStatus? status,
        CancellationToken ct = default)
    {
        var list = await reservations.GetByUserIdAsync(userId, status, ct);
        var result = new List<UserReservationDto>(list.Count);

        foreach (var r in list)
        {
            var slot = await slots.GetByIdWithReservationsAsync(r.SlotId, ct);
            var resource = await resources.GetByIdAsync(slot!.ResourceId, ct);

            result.Add(new UserReservationDto(
                r.Id,
                slot!.SlotNumber,
                resource!.Name,
                r.Status.ToString(),
                r.ExpiresAt));
        }

        return result;
    }
}