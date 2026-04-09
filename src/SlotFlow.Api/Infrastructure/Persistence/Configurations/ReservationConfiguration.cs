using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Infrastructure.Persistence.Configurations
{
    public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("reservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnName("id");

            builder.Property(r => r.SlotId)
                .HasColumnName("slot_id")
                .IsRequired();

            builder.Property(r => r.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(r => r.HeldAt)
                .HasColumnName("held_at")
                .IsRequired();

            builder.Property(r => r.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(r => r.ConfirmedAt)
                .HasColumnName("confirmed_at");

            builder.Property(r => r.CancelledAt)
                .HasColumnName("cancelled_at");

            // Optimistic concurrency con xmin de PostgreSQL
            builder.Property(r => r.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            // Índice para el job de expiración — busca HELD con ExpiresAt pasado
            builder.HasIndex(r => new { r.Status, r.ExpiresAt })
                .HasDatabaseName("ix_reservations_status_expires_at");

            // Índice para RN-08 — verificar hold activo de un usuario en un recurso
            // No podemos indexar ResourceId directamente porque no está en Reservation,
            // pero sí podemos indexar UserId + SlotId + Status
            builder.HasIndex(r => new { r.UserId, r.SlotId, r.Status })
                .HasDatabaseName("ix_reservations_user_slot_status");
        }
    }
}
