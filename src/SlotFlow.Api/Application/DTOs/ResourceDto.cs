namespace SlotFlow.Api.Application.DTOs
{
    public sealed record ResourceDto(
        Guid Id,
        string Name,
        string Description,
        int HoldDurationMinutes,
        int TotalSlots,
        int AvailableSlots,
        bool IsActive,
        DateTime CreatedAt);
}
