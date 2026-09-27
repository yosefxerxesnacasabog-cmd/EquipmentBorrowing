using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;

    public EfBorrowingRepository(EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountActiveByStudentIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .CountAsync(
                borrowing =>
                    borrowing.StudentId == studentId &&
                    borrowing.Status == BorrowingStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Borrowings.AddAsync(borrowing, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .AsNoTracking()
            .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetActiveByEquipmentIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .FirstOrDefaultAsync(
                borrowing =>
                    borrowing.EquipmentId == equipmentId &&
                    borrowing.Status == BorrowingStatus.Active,
                cancellationToken);
    }
}