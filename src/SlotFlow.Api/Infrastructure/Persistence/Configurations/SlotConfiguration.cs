using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Infrastructure.Persistence.Configurations
{
    public sealed class SlotConfiguration : IEntityTypeConfiguration<Slot>
    {
        public void Configure(EntityTypeBuilder<Slot> builder)
        {
            builder.ToTable("slots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasColumnName("id");

            builder.Property(s => s.ResourceId)
                .HasColumnName("resource_id")
                .IsRequired();

            builder.Property(s => s.SlotNumber)
                .HasColumnName("slot_number")
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Índice compuesto para consultas de disponibilidad por recurso
            builder.HasIndex(s => new { s.ResourceId, s.SlotNumber })
                .HasDatabaseName("ix_slots_resource_slot_number");

            // Relación con Reservations
            builder.HasMany(s => s.Reservations)
                .WithOne()
                .HasForeignKey(r => r.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

            // Acceso al campo privado _reservations
            builder.Navigation(s => s.Reservations)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
