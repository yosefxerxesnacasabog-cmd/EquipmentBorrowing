using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment = new()
    {
        new Equipment(1, "Laptop", true),
        new Equipment(2, "Projector", true),
        new Equipment(3, "Microscope", false)
    };

    public Task<Equipment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var equipment = _equipment.FirstOrDefault(e => e.Id == id);

        return Task.FromResult(equipment);
    }
}