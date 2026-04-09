using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Application.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot?> GetByIdWithReservationsAsync(Guid id, CancellationToken ct = default);
        Task<List<Slot>> GetByResourceIdWithActiveReservationsAsync(
            Guid resourceId, CancellationToken ct = default);
    }
}
