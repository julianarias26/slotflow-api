namespace SlotFlow.Api.Domain.Errors
{
    public static class DomainErrors
    {
        public static class Resource
        {
            public static readonly Error NotFound =
                new("Resource.NotFound", "The resource was not found.");

            public static readonly Error NameAlreadyExists =
                new("Resource.NameAlreadyExists", "A resource with this name already exists.");

            public static readonly Error NotActive =
                new("Resource.NotActive", "The resource is not active.");
        }

        public static class Slot
        {
            public static readonly Error NotFound =
                new("Slot.NotFound", "The slot was not found.");

            public static readonly Error NotAvailable =
                new("Slot.NotAvailable", "The slot is not available for reservation.");
        }

        public static class Reservation
        {
            public static readonly Error NotFound =
                new("Reservation.NotFound", "The reservation was not found.");

            public static readonly Error AlreadyExpired =
                new("Reservation.AlreadyExpired", "The reservation has already expired.");

            public static readonly Error AlreadyConfirmed =
                new("Reservation.AlreadyConfirmed", "The reservation is already confirmed.");

            public static readonly Error AlreadyCancelled =
                new("Reservation.AlreadyCancelled", "The reservation has already been cancelled.");

            public static readonly Error NotOwnedByUser =
                new("Reservation.NotOwnedByUser", "You do not own this reservation.");

            public static readonly Error ActiveHoldExists =
                new("Reservation.ActiveHoldExists",
                    "You already have an active hold for a slot in this resource.");
        }
    }
}
