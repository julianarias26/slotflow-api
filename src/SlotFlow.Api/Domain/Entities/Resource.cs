namespace SlotFlow.Api.Domain.Entities
{
    public sealed class Resource
    {
        private readonly List<Slot> _slots = [];

        private Resource() { } // EF Core

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public TimeSpan HoldDuration { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyList<Slot> Slots => _slots.AsReadOnly();

        public static Resource Create(string name, string description, TimeSpan holdDuration)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Resource name cannot be empty.", nameof(name));

            if (holdDuration < TimeSpan.FromMinutes(1) || holdDuration > TimeSpan.FromHours(24))
                throw new ArgumentOutOfRangeException(nameof(holdDuration),
                    "Hold duration must be between 1 minute and 24 hours.");

            return new Resource
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Description = description.Trim(),
                HoldDuration = holdDuration,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddSlots(int count)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot add slots to an inactive resource.");

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

            var nextNumber = _slots.Count + 1;
            for (var i = 0; i < count; i++)
                _slots.Add(Slot.Create(Id, nextNumber + i));
        }

        public void Deactivate() => IsActive = false;
    }
}
