using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository(PostgresOperations operations) : ILendingRepository
{
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync() =>
        operations.ReadAsync<IReadOnlyList<Borrowing>>(async db => await db.Borrowings.AsNoTracking()
            .Include(loan => loan.Student)
            .Include(loan => loan.Equipment)
            .Where(loan => loan.Status == BorrowingStatus.Active)
            .OrderByDescending(loan => loan.DateBorrowed).ThenBy(loan => loan.Id)
            .ToListAsync());

    public Task<WriteResult> CheckoutAsync(int studentId, int equipmentId, DateTime expectedReturnDate) =>
        operations.WriteAsync(async db =>
        {
            var student = await db.Students.FindAsync(studentId);
            if (student is null)
                return new(false, "The student no longer exists.", true);
            if (!student.IsAllowedToBorrow)
                return new(false, "This student is restricted from borrowing.", true);

            var today = DateTime.Today;
            if (expectedReturnDate <= today)
                return new(false, "Select an expected return date after today.");

            var item = await db.Equipment.FindAsync(equipmentId);
            if (item is null)
                return new(false, "The selected item no longer exists.", true);
            if (!item.IsAvailable || await db.Borrowings.AnyAsync(loan =>
                    loan.EquipmentId == equipmentId && loan.Status == BorrowingStatus.Active))
                return new(false, "The selected item is no longer available.", true);

            var active = await db.Borrowings.CountAsync(loan =>
                loan.StudentId == studentId && loan.Status == BorrowingStatus.Active);
            if (active >= 3)
                return new(false, "This student already has three active items.", true);

            item.MarkAsBorrowed();
            db.Borrowings.Add(new Borrowing(student, item, today, expectedReturnDate));
            return new(true, "Equipment borrowed successfully.");
        });

    public Task<WriteResult> ReturnAsync(int borrowingId) =>
        operations.WriteAsync(async db =>
        {
            var loan = await db.Borrowings.Include(entry => entry.Equipment)
                .SingleOrDefaultAsync(entry => entry.Id == borrowingId);
            if (loan is null)
                return new(false, "The selected borrowing no longer exists.", true);
            if (loan.Status != BorrowingStatus.Active)
                return new(false, "This borrowing has already been returned.", true);

            var today = DateTime.Today;
            if (today < loan.DateBorrowed.Date)
                return new(false, "The return date cannot precede checkout. Check the workstation date.");

            loan.MarkAsReturned(today);
            return new(true, "Equipment returned successfully.");
        });
}
