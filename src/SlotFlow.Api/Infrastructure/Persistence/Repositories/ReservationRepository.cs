using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Enums;

namespace SlotFlow.Api.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository(AppDbContext db) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Reservations.FindAsync([id], ct);

    public async Task<bool> HasActiveHoldAsync(
        string userId, Guid resourceId, CancellationToken ct = default) =>
        await db.Reservations
            .Where(r => r.UserId == userId && r.Status == ReservationStatus.Held)
            .Join(db.Slots.Where(s => s.ResourceId == resourceId),
                r => r.SlotId,
                s => s.Id,
                (r, s) => r)
            .AnyAsync(ct);

    public async Task<List<Reservation>> GetExpiredHoldsAsync(CancellationToken ct = default) =>
        await db.Reservations
            .Where(r => r.Status == ReservationStatus.Held && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<List<Reservation>> GetByUserIdAsync(
        string userId, ReservationStatus? status, CancellationToken ct = default)
    {
        var query = db.Reservations.Where(r => r.UserId == userId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.HeldAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await db.Reservations.AddAsync(reservation, ct);
}