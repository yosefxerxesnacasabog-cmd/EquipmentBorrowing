using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentManagementRepository
{
    Task<WriteResult> RegisterAsync(Equipment equipment);
    Task<WriteResult> UpdateAsync(Equipment original, string identifier, string name, string? description);
}
