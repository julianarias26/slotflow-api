using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlotFlow.Api.Infrastructure.Persistence;

namespace SlotFlow.Api.Infrastructure.BackgroundJobs;

public sealed class ReservationExpiryJob(
    IServiceScopeFactory scopeFactory,
    IOptions<ExpiryJobOptions> options,
    ILogger<ReservationExpiryJob> logger) : BackgroundService
{
    private readonly TimeSpan _interval =
        TimeSpan.FromSeconds(options.Value.IntervalSeconds);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "ReservationExpiryJob started. Interval: {Interval}s",
            _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            await ExpireHoldsAsync(stoppingToken);
        }
    }

    private async Task ExpireHoldsAsync(CancellationToken ct)
    {
        // Cada ejecución usa su propio scope para obtener un DbContext fresco
        // IHostedService es singleton — no puede recibir DbContext por constructor
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            var expired = await db.Reservations
                .Where(r =>
                    r.Status == Domain.Enums.ReservationStatus.Held &&
                    r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(ct);

            if (expired.Count == 0)
                return;

            foreach (var reservation in expired)
                reservation.Expire();

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "ReservationExpiryJob expired {Count} reservation(s) at {Time}",
                expired.Count,
                DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // No relanzamos — el job debe sobrevivir errores transitorios
            logger.LogError(ex, "ReservationExpiryJob encountered an error");
        }
    }
}