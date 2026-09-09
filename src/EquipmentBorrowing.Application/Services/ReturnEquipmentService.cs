using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public async Task<bool> ReturnEquipmentAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing =
            await _borrowingRepository.GetActiveByEquipmentIdAsync(
                equipmentId,
                cancellationToken);

        if (borrowing is null)
        {
            return false;
        }

        borrowing.MarkAsReturned();

        return true;
    }
}