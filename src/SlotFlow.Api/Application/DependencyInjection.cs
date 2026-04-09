using FluentValidation;
using SlotFlow.Api.Api.Validators;
using SlotFlow.Api.Application.UseCases.Reservations;
using SlotFlow.Api.Application.UseCases.Resources;
using SlotFlow.Api.Application.UseCases.Slots;

namespace SlotFlow.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Casos de uso — Resources
        services.AddScoped<CreateResource>();
        services.AddScoped<GetResources>();
        services.AddScoped<GetResourceById>();

        // Casos de uso — Slots
        services.AddScoped<GetSlotsByResource>();

        // Casos de uso — Reservations
        services.AddScoped<HoldSlot>();
        services.AddScoped<ConfirmReservation>();
        services.AddScoped<CancelReservation>();
        services.AddScoped<GetReservationById>();
        services.AddScoped<GetUserReservations>();

        // FluentValidation
        services.AddScoped<IValidator<CreateResourceRequest>,
            CreateResourceRequestValidator>();
        services.AddScoped<IValidator<HoldSlotRequest>,
            HoldSlotRequestValidator>();

        return services;
    }
}