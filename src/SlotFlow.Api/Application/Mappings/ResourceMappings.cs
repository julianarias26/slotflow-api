using SlotFlow.Api.Application.DTOs;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Application.Mappings;

public static class ResourceMappings
{
    public static ResourceDto ToDto(this Resource resource)
    {
        var availableSlots = resource.Slots.Count(s => s.IsAvailable());

        return new ResourceDto(
            resource.Id,
            resource.Name,
            resource.Description,
            (int)resource.HoldDuration.TotalMinutes,
            resource.Slots.Count,
            availableSlots,
            resource.IsActive,
            resource.CreatedAt);
    }
}