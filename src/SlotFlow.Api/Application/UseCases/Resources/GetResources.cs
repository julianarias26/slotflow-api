using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;

namespace SlotFlow.Api.Application.UseCases.Resources;

public sealed class GetResources(IResourceRepository resources)
{
    public async Task<List<ResourceDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var list = await resources.GetAllActiveAsync(ct);
        return list.Select(r => r.ToDto()).ToList();
    }
}