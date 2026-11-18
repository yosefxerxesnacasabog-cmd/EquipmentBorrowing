using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(student => student.Id);
        builder.Property(student => student.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 100);
        builder.Property(student => student.StudentNumber).IsRequired();
        builder.Property(student => student.Name).IsRequired();
        builder.Property(student => student.IsAllowedToBorrow).IsRequired();
        builder.Property<string>("NormalizedStudentNumber")
            .HasComputedColumnSql("lower(regexp_replace(\"StudentNumber\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true);
        builder.HasIndex("NormalizedStudentNumber").IsUnique();
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Students_Number_NotBlank", "\"NormalizedStudentNumber\" <> ''");
            table.HasCheckConstraint("CK_Students_Name_NotBlank", "\"Name\" ~ '[^[:space:]]'");
            table.HasCheckConstraint("CK_Students_Contact_NotBlank", "\"Contact\" IS NULL OR \"Contact\" ~ '[^[:space:]]'");
        });

        builder.HasData(
            new { Id = 1, StudentNumber = "STU-001", Name = "Juan Dela Cruz", IsAllowedToBorrow = true },
            new { Id = 2, StudentNumber = "STU-002", Name = "Maria Santos", IsAllowedToBorrow = true },
            new { Id = 3, StudentNumber = "STU-003", Name = "Pedro Reyes", IsAllowedToBorrow = false });
    }
}
