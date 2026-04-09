using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Enums;

namespace SlotFlow.Api.Infrastructure.Persistence.Repositories;

public sealed class SlotRepository(AppDbContext db) : ISlotRepository
{
    public async Task<Slot?> GetByIdWithReservationsAsync(Guid id, CancellationToken ct = default) =>
        await db.Slots
            .Include(s => s.Reservations.Where(r =>
                r.Status == ReservationStatus.Held ||
                r.Status == ReservationStatus.Confirmed))
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Slot>> GetByResourceIdWithActiveReservationsAsync(
        Guid resourceId, CancellationToken ct = default) =>
        await db.Slots
            .Where(s => s.ResourceId == resourceId)
            .Include(s => s.Reservations.Where(r =>
                r.Status == ReservationStatus.Held ||
                r.Status == ReservationStatus.Confirmed))
            .OrderBy(s => s.SlotNumber)
            .ToListAsync(ct);
}