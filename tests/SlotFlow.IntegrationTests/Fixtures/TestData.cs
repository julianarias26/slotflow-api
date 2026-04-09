using System.Net.Http.Json;

namespace SlotFlow.IntegrationTests.Fixtures;

public static class TestData
{
    public static async Task<(Guid ResourceId, List<Guid> SlotIds)> CreateResourceWithSlotsAsync(
        HttpClient client,
        string name = "Test Resource",
        int holdDurationMinutes = 10,
        int slotCount = 5)
    {
        var response = await client.PostAsJsonAsync("/api/resources", new
        {
            name,
            description = "Integration test resource",
            holdDurationMinutes,
            initialSlotCount = slotCount
        });

        response.EnsureSuccessStatusCode();
        var resource = await response.Content.ReadFromJsonAsync<ResourceResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize resource");

        var slotsResponse = await client.GetFromJsonAsync<List<SlotResponse>>(
            $"/api/resources/{resource.Id}/slots")
            ?? [];

        return (resource.Id, slotsResponse.Select(s => s.Id).ToList());
    }

    public sealed record ResourceResponse(Guid Id, string Name, int AvailableSlots);
    public sealed record SlotResponse(Guid Id, int SlotNumber, bool IsAvailable);
    public sealed record ReservationResponse(
        Guid Id, string Status, DateTime ExpiresAt, Guid SlotId);
}