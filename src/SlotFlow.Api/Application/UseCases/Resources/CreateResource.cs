using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Application.Mappings;
using SlotFlow.Api.Common;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Errors;

namespace SlotFlow.Api.Application.UseCases.Resources;

public sealed class CreateResource(
    IResourceRepository resources,
    IUnitOfWork uow)
{
    public sealed record Request(
        string Name,
        string Description,
        int HoldDurationMinutes,
        int InitialSlotCount);

    public async Task<Result<ResourceDto>> ExecuteAsync(
        Request request, CancellationToken ct = default)
    {
        if (await resources.ExistsByNameAsync(request.Name, ct))
            return Result<ResourceDto>.Failure(DomainErrors.Resource.NameAlreadyExists);

        var resource = Resource.Create(
            request.Name,
            request.Description,
            TimeSpan.FromMinutes(request.HoldDurationMinutes));

        resource.AddSlots(request.InitialSlotCount);

        await resources.AddAsync(resource, ct);
        await uow.SaveChangesAsync(ct);

        return Result<ResourceDto>.Success(resource.ToDto());
    }
}