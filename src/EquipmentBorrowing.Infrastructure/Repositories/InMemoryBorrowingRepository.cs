using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();

    public Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b =>
            b.Student.Id == studentId &&
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var activeBorrowings = _borrowings
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToList();

        return Task.FromResult<IReadOnlyList<Borrowing>>(activeBorrowings);
    }

    public Task<Borrowing?> GetActiveByEquipmentIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b =>
            b.Equipment.Id == equipmentId &&
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(borrowing);
    }
}