using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Errors;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.Api.Application.UseCases.Reservations;

public sealed class HoldSlot(
    ISlotRepository slots,
    IResourceRepository resources,
    IReservationRepository reservations,
    IUnitOfWork uow)
{
    public sealed record Request(Guid SlotId, string UserId);

    public async Task<Result<ReservationDto>> ExecuteAsync(
        Request request, CancellationToken ct = default)
    {
        var slot = await slots.GetByIdWithReservationsAsync(request.SlotId, ct);

        if (slot is null)
            return Result<ReservationDto>.Failure(DomainErrors.Slot.NotFound);

        var resource = await resources.GetByIdAsync(slot.ResourceId, ct);

        if (resource is null)
            return Result<ReservationDto>.Failure(DomainErrors.Resource.NotFound);

        if (!resource.IsActive)
            return Result<ReservationDto>.Failure(DomainErrors.Resource.NotActive);

        // RN-08: verificar que el usuario no tenga un hold activo en este recurso
        var hasActiveHold = await reservations.HasActiveHoldAsync(
            request.UserId, slot.ResourceId, ct);

        if (hasActiveHold)
            return Result<ReservationDto>.Failure(DomainErrors.Reservation.ActiveHoldExists);

        try
        {
            var reservation = slot.Reserve(request.UserId, resource.HoldDuration);
            await reservations.AddAsync(reservation, ct);
            await uow.SaveChangesAsync(ct);

            return Result<ReservationDto>.Success(
                reservation.ToDto(slot, resource));
        }
        catch (DbUpdateConcurrencyException)
        {
            // Dos usuarios intentaron reservar el mismo slot simultáneamente
            // El optimistic concurrency de EF Core detectó el conflicto
            return Result<ReservationDto>.Failure(DomainErrors.Slot.NotAvailable);
        }
        catch (DomainException ex)
        {
            return Result<ReservationDto>.Failure(ex.Error);
        }
    }
}