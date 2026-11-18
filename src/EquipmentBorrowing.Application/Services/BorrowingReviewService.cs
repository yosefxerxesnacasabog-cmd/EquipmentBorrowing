using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class BorrowingReviewService(IBorrowingReviewRepository repository)
{
    public Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearch search, CancellationToken cancellationToken = default) =>
        repository.SearchAsync(search with { Text = search.Text?.Trim() }, cancellationToken);

    public Task<IReadOnlyList<Borrowing>?> GetStudentHistoryAsync(int studentId, CancellationToken cancellationToken = default) =>
        repository.GetStudentHistoryAsync(studentId, cancellationToken);

    public Task<IReadOnlyList<Borrowing>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken = default) =>
        repository.GetEquipmentHistoryAsync(equipmentId, cancellationToken);
}
