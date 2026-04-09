namespace SlotFlow.Api.Application.DTOs
{
    public sealed record SlotAvailabilityDto(
        Guid Id,
        int SlotNumber,
        bool IsAvailable,
        ActiveReservationDto? ActiveReservation);

    public sealed record ActiveReservationDto(
        Guid Id,
        string Status,
        DateTime? ExpiresAt,
        string UserId);
}
