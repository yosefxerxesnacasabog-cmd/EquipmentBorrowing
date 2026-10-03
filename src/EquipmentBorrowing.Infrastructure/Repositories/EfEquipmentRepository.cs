using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;

    public EfEquipmentRepository(EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Equipment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Equipment
            .FirstOrDefaultAsync(
                equipment => equipment.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Equipment
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Equipment
            .AsNoTracking()
            .Where(equipment => equipment.IsAvailable)
            .ToListAsync(cancellationToken);
    }
}