using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository(PostgresOperations operations) : IEquipmentRepository
{
    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        operations.ReadAsync(db => db.Equipment.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken));

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetEquipmentAsync(false, cancellationToken);

    public Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default) =>
        GetEquipmentAsync(true, cancellationToken);

    private Task<IReadOnlyList<Equipment>> GetEquipmentAsync(bool availableOnly, CancellationToken cancellationToken) =>
        operations.ReadAsync<IReadOnlyList<Equipment>>(async db => await db.Equipment.AsNoTracking()
            .Where(item => !availableOnly || item.IsAvailable)
            .OrderBy(item => item.Name).ThenBy(item => item.Id)
            .ToListAsync(cancellationToken));
}
