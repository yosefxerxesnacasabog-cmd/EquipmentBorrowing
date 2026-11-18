using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(equipment => equipment.Id);
        builder.Property(equipment => equipment.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 100);
        builder.Property(equipment => equipment.Identifier).IsRequired();
        builder.Property(equipment => equipment.Name).IsRequired();
        builder.Property(equipment => equipment.IsAvailable).IsRequired();
        builder.Property<string>("NormalizedIdentifier")
            .HasComputedColumnSql("lower(regexp_replace(\"Identifier\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true);
        builder.HasIndex("NormalizedIdentifier").IsUnique();
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Equipment_Identifier_NotBlank", "\"NormalizedIdentifier\" <> ''");
            table.HasCheckConstraint("CK_Equipment_Name_NotBlank", "\"Name\" ~ '[^[:space:]]'");
            table.HasCheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR \"Description\" ~ '[^[:space:]]'");
        });

        builder.HasData(
            new { Id = 1, Identifier = "EQ-001", Name = "Laptop", IsAvailable = true },
            new { Id = 2, Identifier = "EQ-002", Name = "Projector", IsAvailable = true },
            new { Id = 3, Identifier = "EQ-003", Name = "Microscope", IsAvailable = true },
            new { Id = 4, Identifier = "EQ-004", Name = "Tablet", IsAvailable = true });
    }
}
