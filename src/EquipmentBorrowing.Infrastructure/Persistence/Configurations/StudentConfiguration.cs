using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(student => student.Id);

        builder.Property(student => student.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(student => student.IsAllowedToBorrow)
            .IsRequired();
    }
}