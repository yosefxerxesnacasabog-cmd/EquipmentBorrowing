using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment = new();

    public Task<Equipment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var equipment = _equipment.FirstOrDefault(e => e.Id == id);

        return Task.FromResult(equipment);
    }
}