using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class EquipmentManagementService(IEquipmentManagementRepository repository)
{
    public Task<WriteResult> SaveAsync(Equipment? original, string? identifier, string? name, string? description)
    {
        identifier = identifier?.Trim();
        name = name?.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (string.IsNullOrEmpty(identifier))
            return Task.FromResult(new WriteResult(false, "Enter the equipment identifier."));
        if (string.IsNullOrEmpty(name))
            return Task.FromResult(new WriteResult(false, "Enter the equipment name."));

        return original is null
            ? repository.RegisterAsync(new Equipment(identifier, name, description))
            : repository.UpdateAsync(original, identifier, name, description);
    }
}
