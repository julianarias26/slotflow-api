using FluentAssertions;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Enums;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.UnitTests.Domain
{
    public sealed class ReservationTests
    {
        private static Slot CreateAvailableSlot()
        {
            var resource = Resource.Create("Taller", "Desc", TimeSpan.FromMinutes(10));
            resource.AddSlots(1);
            return resource.Slots[0];
        }

        private static Reservation CreateHeldReservation(TimeSpan? holdDuration = null) =>
            CreateAvailableSlot().Reserve("user-1", holdDuration ?? TimeSpan.FromMinutes(10));

        // --- Confirm ---

        [Fact]
        public void Confirm_OnHeldNonExpiredReservation_SetsStatusToConfirmed()
        {
            var reservation = CreateHeldReservation();

            reservation.Confirm();

            reservation.Status.Should().Be(ReservationStatus.Confirmed);
            reservation.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Confirm_OnAlreadyConfirmedReservation_IsIdempotent()
        {
            var reservation = CreateHeldReservation();
            reservation.Confirm();
            var confirmedAt = reservation.ConfirmedAt;

            // Segunda llamada — no debe lanzar excepción ni cambiar el timestamp
            var act = () => reservation.Confirm();

            act.Should().NotThrow();
            reservation.Status.Should().Be(ReservationStatus.Confirmed);
            reservation.ConfirmedAt.Should().Be(confirmedAt);
        }

        [Fact]
        public void Confirm_OnExpiredReservation_ThrowsDomainException()
        {
            var reservation = CreateHeldReservation();
            reservation.Expire();

            var act = () => reservation.Confirm();

            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("Reservation.AlreadyExpired");
        }

        [Fact]
        public void Confirm_OnHeldButTimeElapsed_ThrowsDomainException()
        {
            // Simula una reserva cuyo tiempo ya pasó pero el job aún no la expiró
            var reservation = CreateHeldReservation(TimeSpan.FromMilliseconds(1));
            Thread.Sleep(10); // garantiza que ExpiresAt < UtcNow

            var act = () => reservation.Confirm();

            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("Reservation.AlreadyExpired");
        }

        [Fact]
        public void Confirm_OnReleasedReservation_ThrowsDomainException()
        {
            var reservation = CreateHeldReservation();
            reservation.Cancel();

            var act = () => reservation.Confirm();

            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("Reservation.AlreadyCancelled");
        }

        // --- Cancel ---

        [Fact]
        public void Cancel_OnHeldReservation_SetsStatusToReleased()
        {
            var reservation = CreateHeldReservation();

            reservation.Cancel();

            reservation.Status.Should().Be(ReservationStatus.Released);
            reservation.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Cancel_OnConfirmedReservation_ThrowsDomainException()
        {
            var reservation = CreateHeldReservation();
            reservation.Confirm();

            var act = () => reservation.Cancel();

            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("Reservation.AlreadyConfirmed");
        }

        [Fact]
        public void Cancel_OnExpiredReservation_DoesNotThrow()
        {
            var reservation = CreateHeldReservation();
            reservation.Expire();

            var act = () => reservation.Cancel();

            act.Should().NotThrow();
            reservation.Status.Should().Be(ReservationStatus.Expired); // no cambia
        }

        [Fact]
        public void Cancel_OnAlreadyReleasedReservation_DoesNotThrow()
        {
            var reservation = CreateHeldReservation();
            reservation.Cancel();

            var act = () => reservation.Cancel();

            act.Should().NotThrow();
            reservation.Status.Should().Be(ReservationStatus.Released); // no cambia
        }

        // --- Expire ---

        [Fact]
        public void Expire_OnHeldReservation_SetsStatusToExpired()
        {
            var reservation = CreateHeldReservation();

            reservation.Expire();

            reservation.Status.Should().Be(ReservationStatus.Expired);
            reservation.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Expire_OnConfirmedReservation_DoesNotChangeStatus()
        {
            var reservation = CreateHeldReservation();
            reservation.Confirm();

            reservation.Expire();

            reservation.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [Fact]
        public void Expire_OnAlreadyExpiredReservation_DoesNotThrow()
        {
            var reservation = CreateHeldReservation();
            reservation.Expire();

            var act = () => reservation.Expire();

            act.Should().NotThrow();
            reservation.Status.Should().Be(ReservationStatus.Expired);
        }

        // --- IsExpired ---

        [Fact]
        public void IsExpired_WhenHeldAndExpiresAtInFuture_ReturnsFalse()
        {
            var reservation = CreateHeldReservation(TimeSpan.FromMinutes(10));

            reservation.IsExpired().Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WhenHeldAndExpiresAtPassed_ReturnsTrue()
        {
            var reservation = CreateHeldReservation(TimeSpan.FromMilliseconds(1));
            Thread.Sleep(10);

            reservation.IsExpired().Should().BeTrue();
        }

        [Fact]
        public void IsExpired_WhenConfirmed_ReturnsFalse()
        {
            var reservation = CreateHeldReservation();
            reservation.Confirm();

            reservation.IsExpired().Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WhenAlreadyMarkedAsExpired_ReturnsFalse()
        {
            // Una vez que el job la expira, Status == Expired → IsExpired() retorna false
            // porque la condición requiere Status == Held
            var reservation = CreateHeldReservation(TimeSpan.FromMilliseconds(1));
            Thread.Sleep(10);
            reservation.Expire();

            reservation.IsExpired().Should().BeFalse();
        }
    }
}
