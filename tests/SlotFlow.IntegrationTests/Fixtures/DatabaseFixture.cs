using Microsoft.Extensions.DependencyInjection;
using SlotFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SlotFlow.IntegrationTests.Fixtures;

public static class DatabaseFixture
{
    public static async Task ResetAsync(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Borrar en orden correcto respetando FK constraints
        await db.Reservations.ExecuteDeleteAsync();
        await db.Slots.ExecuteDeleteAsync();
        await db.Resources.ExecuteDeleteAsync();
    }
}