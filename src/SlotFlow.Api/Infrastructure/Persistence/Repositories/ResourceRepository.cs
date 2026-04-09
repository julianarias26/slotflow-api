using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Infrastructure.Persistence.Repositories;

public sealed class ResourceRepository(AppDbContext db) : IResourceRepository
{
    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Resources.FindAsync([id], ct);

    public async Task<Resource?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default) =>
    await db.Resources
        .Include(r => r.Slots)
            .ThenInclude(s => s.Reservations.Where(res =>
                res.Status == Domain.Enums.ReservationStatus.Held ||
                res.Status == Domain.Enums.ReservationStatus.Confirmed))
        .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<Resource>> GetAllActiveAsync(CancellationToken ct = default) =>
        await db.Resources
            .Where(r => r.IsActive)
            .Include(r => r.Slots)
                .ThenInclude(s => s.Reservations.Where(res =>
                    res.Status == Domain.Enums.ReservationStatus.Held ||
                    res.Status == Domain.Enums.ReservationStatus.Confirmed))
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        await db.Resources.AnyAsync(r => r.Name == name, ct);

    public async Task AddAsync(Resource resource, CancellationToken ct = default) =>
        await db.Resources.AddAsync(resource, ct);
}