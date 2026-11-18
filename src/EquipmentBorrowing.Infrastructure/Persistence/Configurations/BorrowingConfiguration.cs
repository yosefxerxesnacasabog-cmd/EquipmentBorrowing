using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.HasKey(borrowing => borrowing.Id);

        builder.Property(borrowing => borrowing.DateBorrowed).HasColumnType("date")
            .IsRequired();

        builder.Property(borrowing => borrowing.ExpectedReturnDate).HasColumnType("date")
            .IsRequired();

        builder.Property(borrowing => borrowing.ActualReturnDate).HasColumnType("date");
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Borrowings_ExpectedAfterCheckout", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
            table.HasCheckConstraint("CK_Borrowings_ReturnStatus", """
                ("Status" = 'Active' AND "ActualReturnDate" IS NULL) OR
                ("Status" = 'Returned' AND "ActualReturnDate" IS NOT NULL AND "ActualReturnDate" >= "DateBorrowed")
                """);
        });
        builder.Property(borrowing => borrowing.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(borrowing => borrowing.Student)
            .WithMany()
            .HasForeignKey(borrowing => borrowing.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(borrowing => borrowing.Equipment)
            .WithMany()
            .HasForeignKey(borrowing => borrowing.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(borrowing => borrowing.StudentId);
        builder.HasIndex(borrowing => borrowing.EquipmentId, "IX_Borrowings_ActiveEquipment")
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");
        builder.HasIndex(borrowing => borrowing.EquipmentId);
    }
}
