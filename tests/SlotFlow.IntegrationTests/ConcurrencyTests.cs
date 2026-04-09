using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SlotFlow.IntegrationTests.Fixtures;

namespace SlotFlow.IntegrationTests;

public sealed class ConcurrencyTests(ApiFactory factory)
    : IClassFixture<ApiFactory>, IAsyncLifetime
{
    public async Task InitializeAsync() =>
        await DatabaseFixture.ResetAsync(factory);

    public Task DisposeAsync() => Task.CompletedTask;

    //[Fact]
    //public async Task SimultaneousHolds_OnSameSlot_ExactlyOneSucceeds()
    //{
    //    // Arrange
    //    var setupClient = factory.CreateClient();
    //    var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(
    //        setupClient,
    //        name: $"Concurrency Test {Guid.NewGuid()}",
    //        slotCount: 1);

    //    var slotId = slotIds[0];
    //    const int concurrentUsers = 10;

    //    // Crear 10 clientes con diferentes user IDs
    //    var tasks = Enumerable.Range(1, concurrentUsers).Select(i =>
    //    {
    //        var client = factory.CreateClient();
    //        client.DefaultRequestHeaders.TryAddWithoutValidation(
    //            "X-User-Id", $"concurrent-user-{i}");

    //        return client.PostAsJsonAsync("/api/reservations/hold",
    //            new { slotId });
    //    });

    //    // Act — disparar todos simultáneamente
    //    var responses = await Task.WhenAll(tasks);

    //    // Assert — exactamente 1 debe haber tenido éxito
    //    var successful = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
    //    var conflicts = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

    //    successful.Should().Be(1,
    //        because: "only one user can hold the same slot at a time");
    //    conflicts.Should().Be(concurrentUsers - 1,
    //        because: "all other users should receive a conflict response");
    //}

    [Fact]
    public async Task SimultaneousHolds_OnDifferentSlots_AllSucceed()
    {
        // Arrange — un slot por usuario
        const int concurrentUsers = 5;
        var setupClient = factory.CreateClient();
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(
            setupClient,
            name: $"Multi Slot Test {Guid.NewGuid()}",
            slotCount: concurrentUsers);

        var tasks = slotIds.Select((slotId, i) =>
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "X-User-Id", $"multi-user-{i}");

            return client.PostAsJsonAsync("/api/reservations/hold",
                new { slotId });
        });

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert — todos deben haber tenido éxito
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.Created));
    }
}