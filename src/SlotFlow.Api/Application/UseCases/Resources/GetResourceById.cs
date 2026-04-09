using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Errors;

namespace SlotFlow.Api.Application.UseCases.Resources;

public sealed class GetResourceById(IResourceRepository resources)
{
    public async Task<Result<ResourceDto>> ExecuteAsync(
        Guid id, CancellationToken ct = default)
    {
        var resource = await resources.GetByIdAsync(id, ct);

        return resource is null
            ? Result<ResourceDto>.Failure(DomainErrors.Resource.NotFound)
            : Result<ResourceDto>.Success(resource.ToDto());
    }
}