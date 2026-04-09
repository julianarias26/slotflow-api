using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Application.Interfaces
{
    public interface IResourceRepository
    {
        Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Resource?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default);
        Task<List<Resource>> GetAllActiveAsync(CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        Task AddAsync(Resource resource, CancellationToken ct = default);
    }
}
