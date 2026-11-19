using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentManagementRepository
{
    Task<WriteResult> RegisterAsync(Student student);
    Task<WriteResult> UpdateAsync(Student original, string number, string name, string? contact);
    Task<WriteResult> SetEligibilityAsync(int studentId, bool allowed);
}
