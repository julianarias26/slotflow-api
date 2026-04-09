using FluentAssertions;
using SlotFlow.Api.Domain.Entities;
using SlotFlow.Api.Domain.Exceptions;

namespace SlotFlow.UnitTests.Domain
{
    public sealed class ResourceTests
    {
        // --- Create ---

        [Fact]
        public void Create_WithValidData_ReturnsActiveResource()
        {
            var resource = Resource.Create("Taller de fotografía", "Descripción", TimeSpan.FromMinutes(10));

            resource.Id.Should().NotBeEmpty();
            resource.Name.Should().Be("Taller de fotografía");
            resource.Description.Should().Be("Descripción");
            resource.HoldDuration.Should().Be(TimeSpan.FromMinutes(10));
            resource.IsActive.Should().BeTrue();
            resource.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            resource.Slots.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_ThrowsArgumentException(string name)
        {
            var act = () => Resource.Create(name, "Descripción", TimeSpan.FromMinutes(10));

            act.Should().Throw<ArgumentException>()
                .WithParameterName("name");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1441)] // más de 24 horas en minutos
        public void Create_WithInvalidHoldDuration_ThrowsArgumentOutOfRangeException(int minutes)
        {
            var act = () => Resource.Create("Nombre", "Descripción", TimeSpan.FromMinutes(minutes));

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("holdDuration");
        }

        [Fact]
        public void Create_WithMinimumHoldDuration_Succeeds()
        {
            var act = () => Resource.Create("Nombre", "Descripción", TimeSpan.FromMinutes(1));

            act.Should().NotThrow();
        }

        [Fact]
        public void Create_WithMaximumHoldDuration_Succeeds()
        {
            var act = () => Resource.Create("Nombre", "Descripción", TimeSpan.FromHours(24));

            act.Should().NotThrow();
        }

        [Fact]
        public void Create_TrimsNameAndDescription()
        {
            var resource = Resource.Create("  Nombre  ", "  Desc  ", TimeSpan.FromMinutes(10));

            resource.Name.Should().Be("Nombre");
            resource.Description.Should().Be("Desc");
        }

        // --- AddSlots ---

        [Fact]
        public void AddSlots_OnActiveResource_AddsCorrectCount()
        {
            var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));

            resource.AddSlots(5);

            resource.Slots.Should().HaveCount(5);
        }

        [Fact]
        public void AddSlots_AssignsSequentialSlotNumbers()
        {
            var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));

            resource.AddSlots(3);

            resource.Slots.Select(s => s.SlotNumber)
                .Should().BeEquivalentTo([1, 2, 3]);
        }

        [Fact]
        public void AddSlots_CalledTwice_ContinuesNumberingSequentially()
        {
            var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));

            resource.AddSlots(2);
            resource.AddSlots(3);

            resource.Slots.Select(s => s.SlotNumber)
                .Should().BeEquivalentTo([1, 2, 3, 4, 5]);
        }

        //[Fact]
        //public void AddSlots_OnInactiveResource_ThrowsDomainException()
        //{
        //    var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));
        //    resource.Deactivate();

        //    var act = () => resource.AddSlots(5);

        //    act.Should().Throw<DomainException>()
        //        .Which.Error.Code.Should().Be("Resource.NotActive");
        //}

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddSlots_WithInvalidCount_ThrowsArgumentOutOfRangeException(int count)
        {
            var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));

            var act = () => resource.AddSlots(count);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        // --- Deactivate ---

        [Fact]
        public void Deactivate_SetsIsActiveToFalse()
        {
            var resource = Resource.Create("Nombre", "Desc", TimeSpan.FromMinutes(10));

            resource.Deactivate();

            resource.IsActive.Should().BeFalse();
        }
    }
}
