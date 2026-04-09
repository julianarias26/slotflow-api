using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Errors;

namespace SlotFlow.Api.Application.UseCases.Reservations;

public sealed class GetReservationById(
    IReservationRepository reservations,
    ISlotRepository slots,
    IResourceRepository resources)
{
    public sealed record Request(Guid ReservationId, string UserId);

    public async Task<Result<ReservationDto>> ExecuteAsync(
        Request request, CancellationToken ct = default)
    {
        var reservation = await reservations.GetByIdAsync(request.ReservationId, ct);

        if (reservation is null)
            return Result<ReservationDto>.Failure(DomainErrors.Reservation.NotFound);

        if (reservation.UserId != request.UserId)
            return Result<ReservationDto>.Failure(DomainErrors.Reservation.NotOwnedByUser);

        var slot = await slots.GetByIdWithReservationsAsync(reservation.SlotId, ct);
        var resource = await resources.GetByIdAsync(slot!.ResourceId, ct);

        return Result<ReservationDto>.Success(reservation.ToDto(slot!, resource!));
    }
}