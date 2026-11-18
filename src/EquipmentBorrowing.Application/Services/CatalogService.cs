using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class CatalogService(IStudentRepository students, IEquipmentRepository equipment)
{
    public Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default) =>
        students.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(
        bool availableOnly = false, CancellationToken cancellationToken = default) =>
        availableOnly ? equipment.GetAvailableAsync(cancellationToken) : equipment.GetAllAsync(cancellationToken);
}
