using FluentAssertions;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Enums;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.UnitTests.Domain
{
    public sealed class SlotTests
    {
        private static Resource CreateResource() =>
            Resource.Create("Taller", "Desc", TimeSpan.FromMinutes(10));

        // --- IsAvailable ---

        [Fact]
        public void IsAvailable_WithNoReservations_ReturnsTrue()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];

            slot.IsAvailable().Should().BeTrue();
        }

        [Fact]
        public void IsAvailable_WithHeldReservation_ReturnsFalse()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];

            slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            slot.IsAvailable().Should().BeFalse();
        }

        [Fact]
        public void IsAvailable_WithConfirmedReservation_ReturnsFalse()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];
            var reservation = slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            reservation.Confirm();

            slot.IsAvailable().Should().BeFalse();
        }

        [Fact]
        public void IsAvailable_WithExpiredReservation_ReturnsTrue()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];
            var reservation = slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            reservation.Expire();

            slot.IsAvailable().Should().BeTrue();
        }

        [Fact]
        public void IsAvailable_WithReleasedReservation_ReturnsTrue()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];
            var reservation = slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            reservation.Cancel();

            slot.IsAvailable().Should().BeTrue();
        }

        // --- Reserve ---

        [Fact]
        public void Reserve_OnAvailableSlot_ReturnsHeldReservation()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];

            var reservation = slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            reservation.Should().NotBeNull();
            reservation.Status.Should().Be(ReservationStatus.Held);
            reservation.UserId.Should().Be("user-1");
            reservation.SlotId.Should().Be(slot.Id);
            reservation.HeldAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            reservation.ExpiresAt.Should().BeCloseTo(
                DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Reserve_OnAvailableSlot_AddsReservationToSlot()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];

            slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            slot.Reservations.Should().HaveCount(1);
        }

        [Fact]
        public void Reserve_OnUnavailableSlot_ThrowsDomainException()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];
            slot.Reserve("user-1", TimeSpan.FromMinutes(10));

            var act = () => slot.Reserve("user-2", TimeSpan.FromMinutes(10));

            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("Slot.NotAvailable");
        }

        [Fact]
        public void Reserve_AfterPreviousReservationExpired_Succeeds()
        {
            var resource = CreateResource();
            resource.AddSlots(1);
            var slot = resource.Slots[0];
            var first = slot.Reserve("user-1", TimeSpan.FromMinutes(10));
            first.Expire();

            var act = () => slot.Reserve("user-2", TimeSpan.FromMinutes(10));

            act.Should().NotThrow();
        }
    }
}
