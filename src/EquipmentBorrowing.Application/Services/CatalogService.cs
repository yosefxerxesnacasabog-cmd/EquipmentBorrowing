using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class CatalogService(IStudentRepository students, IEquipmentRepository equipment)
{
    public Task<IReadOnlyList<Student>> GetStudentsAsync() => students.GetAllAsync();

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(bool availableOnly = false) =>
        availableOnly ? equipment.GetAvailableAsync() : equipment.GetAllAsync();
}
