using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingReviewRepository
{
    Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearch search, CancellationToken cancellationToken);
    Task<IReadOnlyList<Borrowing>?> GetStudentHistoryAsync(int studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Borrowing>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken);
}

public enum BorrowingSortField { CheckoutDate, ExpectedReturnDate }

public sealed record BorrowingSearch(
    string? Text = null,
    BorrowingStatus? Status = null,
    BorrowingSortField SortField = BorrowingSortField.CheckoutDate,
    bool Descending = true);
