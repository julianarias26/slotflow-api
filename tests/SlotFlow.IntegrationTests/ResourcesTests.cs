using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SlotFlow.IntegrationTests.Fixtures;

namespace SlotFlow.IntegrationTests;

public sealed class ResourcesTests(ApiFactory factory)
    : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() =>
        await DatabaseFixture.ResetAsync(factory);

    public Task DisposeAsync() => Task.CompletedTask;

    //[Fact]
    //public async Task CreateResource_WithValidData_Returns201WithResource()
    //{
    //    var response = await _client.PostAsJsonAsync("/api/resources", new
    //    {
    //        name = "Taller de fotografía",
    //        description = "Cupos para el taller.",
    //        holdDurationMinutes = 10,
    //        initialSlotCount = 5
    //    });

    //    response.StatusCode.Should().Be(HttpStatusCode.Created);

    //    var body = await response.Content.ReadFromJsonAsync<dynamic>();
    //    body.Should().NotBeNull();
    //}

    [Fact]
    public async Task CreateResource_WithDuplicateName_Returns409()
    {
        var payload = new
        {
            name = "Recurso único",
            description = "Desc",
            holdDurationMinutes = 10,
            initialSlotCount = 3
        };

        await _client.PostAsJsonAsync("/api/resources", payload);
        var second = await _client.PostAsJsonAsync("/api/resources", payload);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("Resource.NameAlreadyExists");
    }

    [Fact]
    public async Task CreateResource_WithInvalidHoldDuration_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/resources", new
        {
            name = "Recurso inválido",
            description = "Desc",
            holdDurationMinutes = 0,  // inválido
            initialSlotCount = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body!.Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public async Task GetResources_ReturnsAllActiveResources()
    {
        await TestData.CreateResourceWithSlotsAsync(_client, "Recurso A");
        await TestData.CreateResourceWithSlotsAsync(_client, "Recurso B");

        var response = await _client.GetAsync("/api/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        list.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetResourceById_WithInvalidId_Returns404()
    {
        var response = await _client.GetAsync($"/api/resources/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("Resource.NotFound");
    }

    [Fact]
    public async Task GetSlots_AfterCreation_ReturnsAllAvailable()
    {
        var (resourceId, slotIds) =
            await TestData.CreateResourceWithSlotsAsync(_client, slotCount: 3);

        var response = await _client.GetAsync($"/api/resources/{resourceId}/slots");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var slots = await response.Content
            .ReadFromJsonAsync<List<TestData.SlotResponse>>();

        slots.Should().HaveCount(3);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    // DTOs de respuesta para tests
    public sealed record ErrorResponse(string Code, string Message);
    public sealed record ValidationErrorResponse(string Code, string Message);
}