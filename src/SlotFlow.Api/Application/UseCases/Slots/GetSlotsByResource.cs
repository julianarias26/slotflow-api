using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Enums;
using SlotFlow.Api.Domain.Errors;

namespace SlotFlow.Api.Application.UseCases.Slots;

public sealed class GetSlotsByResource(
    IResourceRepository resources,
    ISlotRepository slots)
{
    public async Task<Result<List<SlotAvailabilityDto>>> ExecuteAsync(
        Guid resourceId, CancellationToken ct = default)
    {
        var resource = await resources.GetByIdAsync(resourceId, ct);

        if (resource is null)
            return Result<List<SlotAvailabilityDto>>.Failure(DomainErrors.Resource.NotFound);

        var slotList = await slots.GetByResourceIdWithActiveReservationsAsync(resourceId, ct);

        var dtos = slotList.Select(s =>
        {
            var active = s.Reservations.FirstOrDefault(r =>
                r.Status == ReservationStatus.Held ||
                r.Status == ReservationStatus.Confirmed);

            return new SlotAvailabilityDto(
                s.Id,
                s.SlotNumber,
                s.IsAvailable(),
                active is null ? null : new ActiveReservationDto(
                    active.Id,
                    active.Status.ToString(),
                    active.Status == ReservationStatus.Held ? active.ExpiresAt : null,
                    active.UserId));
        }).ToList();

        return Result<List<SlotAvailabilityDto>>.Success(dtos);
    }
}