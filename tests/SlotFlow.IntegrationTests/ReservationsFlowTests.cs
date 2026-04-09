using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SlotFlow.IntegrationTests.Fixtures;

namespace SlotFlow.IntegrationTests;

public sealed class ReservationsFlowTests(ApiFactory factory)
    : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() =>
        await DatabaseFixture.ResetAsync(factory);

    public Task DisposeAsync() => Task.CompletedTask;

    // --- Hold ---

    [Fact]
    public async Task Hold_OnAvailableSlot_Returns201WithHeldReservation()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-1");

        var response = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var reservation = await response.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        reservation!.Status.Should().Be("Held");
        reservation.SlotId.Should().Be(slotIds[0]);
    }

    [Fact]
    public async Task Hold_WithoutUserIdHeader_Returns400()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);

        // Sin header X-User-Id
        var clientWithoutHeader = factory.CreateClient();
        var response = await clientWithoutHeader.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Hold_OnAlreadyHeldSlot_Returns409()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);

        var clientA = factory.CreateClient();
        clientA.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-A");

        var clientB = factory.CreateClient();
        clientB.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-B");

        await clientA.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });

        var second = await clientB.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("Slot.NotAvailable");
    }

    [Fact]
    public async Task Hold_WhenUserAlreadyHasActiveHoldOnResource_Returns409()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(
            _client, slotCount: 3);

        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-rn08");

        // Primera reserva exitosa
        await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });

        // Segunda reserva en otro slot del mismo recurso — debe fallar (RN-08)
        var second = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[1] });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("Reservation.ActiveHoldExists");
    }

    // --- Confirm ---

    [Fact]
    public async Task Confirm_OnHeldReservation_Returns200WithConfirmedStatus()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-confirm");

        var holdResponse = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        var reservation = await holdResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        var confirmResponse = await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation!.Id}/confirm", new { });

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmed = await confirmResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();
        confirmed!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Confirm_IsIdempotent_WhenAlreadyConfirmed()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-idem");

        var holdResponse = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        var reservation = await holdResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation!.Id}/confirm", new { });

        // Segunda confirmación — debe ser idempotente
        var second = await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation.Id}/confirm", new { });

        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await second.Content.ReadFromJsonAsync<TestData.ReservationResponse>();
        body!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Confirm_ByDifferentUser_Returns403()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);

        var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "owner");

        var otherClient = factory.CreateClient();
        otherClient.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "stranger");

        var holdResponse = await ownerClient.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        var reservation = await holdResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        var response = await otherClient.PostAsJsonAsync(
            $"/api/reservations/{reservation!.Id}/confirm", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("Reservation.NotOwnedByUser");
    }

    // --- Cancel ---

    [Fact]
    public async Task Cancel_OnHeldReservation_Returns200AndSlotBecomesAvailable()
    {
        var (resourceId, slotIds) =
            await TestData.CreateResourceWithSlotsAsync(_client, slotCount: 1);

        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-cancel");

        var holdResponse = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        var reservation = await holdResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        var cancelResponse = await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation!.Id}/cancel", new { });

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelled = await cancelResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();
        cancelled!.Status.Should().Be("Released");

        // Verificar que el slot volvió a estar disponible
        var slots = await _client.GetFromJsonAsync<List<TestData.SlotResponse>>(
            $"/api/resources/{resourceId}/slots");

        slots![0].IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_OnConfirmedReservation_Returns409()
    {
        var (_, slotIds) = await TestData.CreateResourceWithSlotsAsync(_client);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-cancel2");

        var holdResponse = await _client.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        var reservation = await holdResponse.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation!.Id}/confirm", new { });

        var cancelResponse = await _client.PostAsJsonAsync(
            $"/api/reservations/{reservation.Id}/cancel", new { });

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await cancelResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("Reservation.AlreadyConfirmed");
    }

    // --- Full flow ---

    [Fact]
    public async Task FullFlow_HoldConfirmSlotUnavailableThenAnotherUserCannotHold()
    {
        var (_, slotIds) =
            await TestData.CreateResourceWithSlotsAsync(_client, slotCount: 1);

        var userA = factory.CreateClient();
        userA.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-flow-A");

        var userB = factory.CreateClient();
        userB.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Id", "user-flow-B");

        // A reserva
        var holdA = await userA.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        holdA.StatusCode.Should().Be(HttpStatusCode.Created);

        var reservationA = await holdA.Content
            .ReadFromJsonAsync<TestData.ReservationResponse>();

        // B intenta reservar el mismo slot
        var holdB = await userB.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        holdB.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // A confirma
        var confirm = await userA.PostAsJsonAsync(
            $"/api/reservations/{reservationA!.Id}/confirm", new { });
        confirm.StatusCode.Should().Be(HttpStatusCode.OK);

        // B sigue sin poder reservar
        var holdB2 = await userB.PostAsJsonAsync("/api/reservations/hold",
            new { slotId = slotIds[0] });
        holdB2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    public sealed record ErrorResponse(string Code, string Message);
}