namespace SlotFlow.Api.Application.DTOs;

public sealed record UserReservationDto(
    Guid Id,
    int SlotNumber,
    string ResourceName,
    string Status,
    DateTime ExpiresAt);