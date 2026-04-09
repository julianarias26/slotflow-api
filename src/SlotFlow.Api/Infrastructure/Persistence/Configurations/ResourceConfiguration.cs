using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SlotFlow.Api.Domain.Entities;

namespace SlotFlow.Api.Infrastructure.Persistence.Configurations
{
    public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.ToTable("resources");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnName("id");

            builder.Property(r => r.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(r => r.HoldDuration)
                .HasColumnName("hold_duration")
                .IsRequired();

            builder.Property(r => r.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Índice único en Name — soporta RN de nombre único
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("ix_resources_name");

            // Relación con Slots
            builder.HasMany(r => r.Slots)
                .WithOne()
                .HasForeignKey(s => s.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Acceso al campo privado _slots
            builder.Navigation(r => r.Slots)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
