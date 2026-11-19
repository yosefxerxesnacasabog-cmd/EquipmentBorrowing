using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed partial class EfBorrowingRepository : IBorrowingReviewRepository
{
    public Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearch search, CancellationToken cancellationToken) =>
        operations.ReadAsync<IReadOnlyList<Borrowing>>(async db =>
        {
            var query = WithDetails(db);
            if (search.Status is BorrowingStatus status)
                query = query.Where(loan => loan.Status == status);

            if (!string.IsNullOrEmpty(search.Text))
            {
                var pattern = "%" + search.Text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
                query = query.Where(loan =>
                    EF.Functions.ILike(loan.Student.Name, pattern, "\\") ||
                    EF.Functions.ILike(loan.Student.StudentNumber, pattern, "\\") ||
                    EF.Functions.ILike(loan.Equipment.Name, pattern, "\\"));
            }

            var ordered = (search.SortField, search.Descending) switch
            {
                (BorrowingSortField.ExpectedReturnDate, true) => query.OrderByDescending(loan => loan.ExpectedReturnDate),
                (BorrowingSortField.ExpectedReturnDate, false) => query.OrderBy(loan => loan.ExpectedReturnDate),
                (_, true) => query.OrderByDescending(loan => loan.DateBorrowed),
                _ => query.OrderBy(loan => loan.DateBorrowed)
            };

            return await ordered.ThenBy(loan => loan.Id).ToListAsync(cancellationToken);
        });

    public Task<IReadOnlyList<Borrowing>?> GetStudentHistoryAsync(int studentId, CancellationToken cancellationToken) =>
        operations.ReadAsync<IReadOnlyList<Borrowing>?>(async db =>
        {
            if (!await db.Students.AnyAsync(student => student.Id == studentId, cancellationToken))
                return null;

            return await ByRecentCheckout(WithDetails(db).Where(loan => loan.StudentId == studentId))
                .ToListAsync(cancellationToken);
        });

    public Task<IReadOnlyList<Borrowing>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken) =>
        operations.ReadAsync<IReadOnlyList<Borrowing>?>(async db =>
        {
            if (!await db.Equipment.AnyAsync(item => item.Id == equipmentId, cancellationToken))
                return null;

            return await ByRecentCheckout(WithDetails(db).Where(loan => loan.EquipmentId == equipmentId))
                .ToListAsync(cancellationToken);
        });

    private static IQueryable<Borrowing> WithDetails(EquipmentBorrowingDbContext db) =>
        db.Borrowings.AsNoTracking().Include(loan => loan.Student).Include(loan => loan.Equipment);

    private static IOrderedQueryable<Borrowing> ByRecentCheckout(IQueryable<Borrowing> query) =>
        query.OrderByDescending(loan => loan.DateBorrowed).ThenBy(loan => loan.Id);
}
