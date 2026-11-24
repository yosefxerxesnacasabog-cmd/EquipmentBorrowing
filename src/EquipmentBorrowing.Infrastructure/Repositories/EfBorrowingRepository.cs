using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed partial class EfBorrowingRepository(PostgresOperations operations) : ILendingRepository
{
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync() =>
        operations.ReadAsync<IReadOnlyList<Borrowing>>(async db =>
            await ByRecentCheckout(WithDetails(db).Where(loan => loan.Status == BorrowingStatus.Active)).ToListAsync());

    public Task<WriteResult> CheckoutAsync(int studentId, IReadOnlyList<int> equipmentIds, DateTime expectedReturnDate) =>
        operations.WriteAsync(async db =>
        {
            var student = await db.Students.FindAsync(studentId);
            if (student is null)
                return new(false, "The student no longer exists.", true, WriteFailure.Missing);
            if (!student.IsAllowedToBorrow)
                return new(false, "This student is restricted from borrowing.", true, WriteFailure.Conflict);

            var today = DateTime.Today;
            if (expectedReturnDate <= today)
                return new(false, "Select an expected return date after today.");

            var items = await db.Equipment.Where(item => equipmentIds.Contains(item.Id))
                .OrderBy(item => item.Id).ToListAsync();
            if (items.Count != equipmentIds.Count)
                return new(false, "One or more selected items no longer exist.", true, WriteFailure.Missing);
            if (items.Any(item => !item.IsAvailable) || await db.Borrowings.AnyAsync(loan =>
                    equipmentIds.Contains(loan.EquipmentId) && loan.Status == BorrowingStatus.Active))
                return new(false, "One or more selected items are no longer available.", true, WriteFailure.Conflict);

            var active = await db.Borrowings.CountAsync(loan =>
                loan.StudentId == studentId && loan.Status == BorrowingStatus.Active);
            if (active + items.Count > 3)
                return new(false, $"This checkout would exceed the three-item limit. The student has {active} active item(s).", true, WriteFailure.Conflict);

            var loans = new List<Borrowing>();
            foreach (var item in items)
            {
                item.MarkAsBorrowed();
                var loan = new Borrowing(student, item, today, expectedReturnDate);
                loans.Add(loan);
                db.Borrowings.Add(loan);
            }

            return new(true, items.Count == 1 ? "Equipment borrowed successfully." : $"{items.Count} items borrowed successfully.", Value: loans);
        });

    public Task<WriteResult> ReturnAsync(int borrowingId) =>
        operations.WriteAsync(async db =>
        {
            var loan = await db.Borrowings.Include(entry => entry.Equipment)
                .SingleOrDefaultAsync(entry => entry.Id == borrowingId);
            if (loan is null)
                return new(false, "The selected borrowing no longer exists.", true, WriteFailure.Missing);
            if (loan.Status != BorrowingStatus.Active)
                return new(false, "This borrowing has already been returned.", true, WriteFailure.Conflict);

            var today = DateTime.Today;
            if (today < loan.DateBorrowed.Date)
                return new(false, "The return date cannot precede checkout. Check the workstation date.");

            loan.MarkAsReturned(today);
            return new(true, "Equipment returned successfully.", Value: loan);
        });
}
