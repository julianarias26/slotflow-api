using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Errors;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.Api.Application.UseCases.Reservations;

public sealed class CancelReservation(
    IReservationRepository reservations,
    ISlotRepository slots,
    IResourceRepository resources,
    IUnitOfWork uow)
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

        try
        {
            reservation.Cancel();
            await uow.SaveChangesAsync(ct);

            var slot = await slots.GetByIdWithReservationsAsync(reservation.SlotId, ct);
            var resource = await resources.GetByIdAsync(slot!.ResourceId, ct);

            return Result<ReservationDto>.Success(
                reservation.ToDto(slot!, resource!));
        }
        catch (DomainException ex)
        {
            return Result<ReservationDto>.Failure(ex.Error);
        }
    }
}