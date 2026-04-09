using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Enums;

namespace SlotFlow.Api.Application.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> HasActiveHoldAsync(string userId, Guid resourceId, CancellationToken ct = default);
        Task<List<Reservation>> GetExpiredHoldsAsync(CancellationToken ct = default);
        Task<List<Reservation>> GetByUserIdAsync(
            string userId, ReservationStatus? status, CancellationToken ct = default);
        Task AddAsync(Reservation reservation, CancellationToken ct = default);
    }
}
