using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository(PostgresOperations operations) : IEquipmentRepository, IEquipmentManagementRepository
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

    public Task<WriteResult> RegisterAsync(Equipment equipment) =>
        operations.WriteAsync(db =>
        {
            db.Equipment.Add(equipment);
            return Task.FromResult(new WriteResult(true, "Equipment registered as available.", Value: equipment));
        });

    public Task<WriteResult> UpdateAsync(Equipment original, string identifier, string name, string? description) =>
        operations.WriteAsync(async db =>
        {
            var current = await db.Equipment.FindAsync(original.Id);
            if (current is null)
                return new(false, "This item no longer exists.", true, WriteFailure.Missing);
            if (current.Identifier != original.Identifier || current.Name != original.Name ||
                current.Description != original.Description)
                return new(false, "This item's information changed. Review the refreshed record and try again.", true, WriteFailure.Conflict);

            current.UpdateInformation(identifier, name, description);
            return new(true, "Equipment information updated.", Value: current);
        });
}
