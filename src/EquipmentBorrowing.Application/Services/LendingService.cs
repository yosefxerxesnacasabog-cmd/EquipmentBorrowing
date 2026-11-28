using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class LendingService(ILendingRepository repository, TimeProvider? clock = null)
{
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync() => repository.GetActiveAsync();

    public Task<WriteResult> CheckoutAsync(int? studentId, int? equipmentId, DateTime? expectedReturnDate) =>
        CheckoutCoreAsync(studentId, equipmentId is int id ? [id] : [], expectedReturnDate, minimumItems: 1);

    public Task<WriteResult> CheckoutMultipleAsync(int? studentId, IReadOnlyList<int> equipmentIds, DateTime? expectedReturnDate) =>
        CheckoutCoreAsync(studentId, equipmentIds, expectedReturnDate, minimumItems: 2);

    private async Task<WriteResult> CheckoutCoreAsync(
        int? studentId, IReadOnlyList<int> equipmentIds, DateTime? expectedReturnDate, int minimumItems)
    {
        if (studentId is null)
            return new(false, "Select a student.");
        if (equipmentIds.Distinct().Count() != equipmentIds.Count)
            return new(false, "Select each equipment item only once.");
        if (equipmentIds.Count < minimumItems)
            return new(false, minimumItems == 1 ? "Select equipment." : "Select at least two distinct items.");
        if (equipmentIds.Count > 3)
            return new(false, "A student may have at most three active items.", Failure: WriteFailure.Conflict);
        if (expectedReturnDate is null || expectedReturnDate.Value.Date <= (clock ?? TimeProvider.System).GetLocalNow().Date)
            return new(false, "Select an expected return date after today.");

        return await repository.CheckoutAsync(studentId.Value, equipmentIds.ToArray(), expectedReturnDate.Value.Date);
    }

    public Task<WriteResult> ReturnAsync(int? borrowingId) =>
        borrowingId is int id
            ? repository.ReturnAsync(id)
            : Task.FromResult(new WriteResult(false, "Select an active borrowing."));
}
