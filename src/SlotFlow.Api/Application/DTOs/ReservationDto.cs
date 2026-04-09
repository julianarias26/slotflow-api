namespace SlotFlow.Api.Application.DTOs;

public sealed record ReservationDto(
    Guid Id,
    Guid SlotId,
    int SlotNumber,
    Guid ResourceId,
    string ResourceName,
    string UserId,
    string Status,
    DateTime HeldAt,
    DateTime ExpiresAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt);