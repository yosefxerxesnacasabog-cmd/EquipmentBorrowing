using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(equipment => equipment.Id);

        builder.Property(equipment => equipment.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(equipment => equipment.IsAvailable)
            .IsRequired();
    }
}