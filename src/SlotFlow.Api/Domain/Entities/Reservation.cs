namespace SlotFlow.Api.Domain.Entities
{
    public sealed class Reservation
    {
        private Reservation() { } // EF Core

        public Guid Id { get; private set; }
        public Guid SlotId { get; private set; }
        public string UserId { get; private set; } = string.Empty;
        public Enums.ReservationStatus Status { get; private set; }
        public DateTime HeldAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public uint Version { get; private set; } // optimistic concurrency

        public bool IsExpired() =>
            Status == Enums.ReservationStatus.Held &&
            DateTime.UtcNow > ExpiresAt;

        internal static Reservation Create(Guid slotId, string userId, TimeSpan holdDuration)
        {
            var now = DateTime.UtcNow;
            return new Reservation
            {
                Id = Guid.NewGuid(),
                SlotId = slotId,
                UserId = userId,
                Status = Enums.ReservationStatus.Held,
                HeldAt = now,
                ExpiresAt = now.Add(holdDuration)
            };
        }

        public void Confirm()
        {
            if (Status == Enums.ReservationStatus.Confirmed)
                return; // idempotente — RN-07

            if (Status == Enums.ReservationStatus.Expired || IsExpired())
                throw new Exceptions.DomainException(Errors.DomainErrors.Reservation.AlreadyExpired);

            if (Status == Enums.ReservationStatus.Released)
                throw new Exceptions.DomainException(Errors.DomainErrors.Reservation.AlreadyCancelled);

            Status = Enums.ReservationStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == Enums.ReservationStatus.Confirmed)
                throw new Exceptions.DomainException(Errors.DomainErrors.Reservation.AlreadyConfirmed);

            if (Status is Enums.ReservationStatus.Expired or Enums.ReservationStatus.Released)
                return; // ya está inactiva, no hay nada que cancelar

            Status = Enums.ReservationStatus.Released;
            CancelledAt = DateTime.UtcNow;
        }

        public void Expire()
        {
            if (Status != Enums.ReservationStatus.Held)
                return; // solo expiran las HELD

            Status = Enums.ReservationStatus.Expired;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
