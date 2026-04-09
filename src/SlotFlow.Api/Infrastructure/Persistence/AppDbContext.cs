using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await base.SaveChangesAsync(ct);
}