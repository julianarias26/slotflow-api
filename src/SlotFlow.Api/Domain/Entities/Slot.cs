namespace SlotFlow.Api.Domain.Entities
{
    public sealed class Slot
    {
        private readonly List<Reservation> _reservations = [];

        private Slot() { } // EF Core

        public Guid Id { get; private set; }
        public Guid ResourceId { get; private set; }
        public int SlotNumber { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

        public bool IsAvailable() =>
            !_reservations.Any(r =>
                r.Status == Enums.ReservationStatus.Held ||
                r.Status == Enums.ReservationStatus.Confirmed);

        internal static Slot Create(Guid resourceId, int slotNumber) => new()
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            SlotNumber = slotNumber,
            CreatedAt = DateTime.UtcNow
        };

        public Reservation Reserve(string userId, TimeSpan holdDuration)
        {
            if (!IsAvailable())
                throw new Exceptions.DomainException(Errors.DomainErrors.Slot.NotAvailable);

            var reservation = Reservation.Create(Id, userId, holdDuration);
            _reservations.Add(reservation);
            return reservation;
        }
    }
}
