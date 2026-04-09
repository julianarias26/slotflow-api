using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Application.Interfaces;
using SlotFlow.Api.Infrastructure.BackgroundJobs;
using SlotFlow.Api.Infrastructure.Persistence;
using SlotFlow.Api.Infrastructure.Persistence.Repositories;

namespace SlotFlow.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<AppDbContext>());

        // Repositorios
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<ISlotRepository, SlotRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        // Background job
        services.Configure<ExpiryJobOptions>(
            configuration.GetSection(ExpiryJobOptions.SectionName));
        services.AddHostedService<ReservationExpiryJob>();

        return services;
    }
}