using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.HasKey(borrowing => borrowing.Id);

        builder.Property(borrowing => borrowing.DateBorrowed)
            .IsRequired();

        builder.Property(borrowing => borrowing.ExpectedReturnDate)
            .IsRequired();

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
        builder.HasIndex(borrowing => borrowing.EquipmentId);
    }
}