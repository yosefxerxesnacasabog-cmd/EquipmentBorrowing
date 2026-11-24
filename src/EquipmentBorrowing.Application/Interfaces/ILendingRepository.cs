using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface ILendingRepository
{
    Task<IReadOnlyList<Borrowing>> GetActiveAsync();
    Task<WriteResult> CheckoutAsync(int studentId, IReadOnlyList<int> equipmentIds, DateTime expectedReturnDate);
    Task<WriteResult> ReturnAsync(int borrowingId);
}

public enum WriteFailure { Invalid, Missing, Conflict, Server, Unknown }
public sealed record WriteResult(bool Succeeded, string Message, bool RefreshRequired = false,
    WriteFailure Failure = WriteFailure.Invalid, object? Value = null);
