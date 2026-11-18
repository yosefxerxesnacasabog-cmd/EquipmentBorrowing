using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class LendingService(ILendingRepository repository)
{
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync() => repository.GetActiveAsync();

    public async Task<WriteResult> CheckoutAsync(int? studentId, int? equipmentId, DateTime? expectedReturnDate)
    {
        if (studentId is null)
            return new(false, "Select a student.");
        if (equipmentId is null)
            return new(false, "Select equipment.");
        if (expectedReturnDate is null || expectedReturnDate.Value.Date <= DateTime.Today)
            return new(false, "Select an expected return date after today.");

        return await repository.CheckoutAsync(studentId.Value, equipmentId.Value, expectedReturnDate.Value.Date);
    }

    public Task<WriteResult> ReturnAsync(int? borrowingId) =>
        borrowingId is int id
            ? repository.ReturnAsync(id)
            : Task.FromResult(new WriteResult(false, "Select an active borrowing."));
}
