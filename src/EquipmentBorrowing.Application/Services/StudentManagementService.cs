using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class StudentManagementService(IStudentManagementRepository repository)
{
    public Task<WriteResult> SaveAsync(Student? original, string? number, string? name, string? contact)
    {
        number = number?.Trim();
        name = name?.Trim();
        contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (string.IsNullOrEmpty(number))
            return Task.FromResult(new WriteResult(false, "Enter the institutional student number."));
        if (string.IsNullOrEmpty(name))
            return Task.FromResult(new WriteResult(false, "Enter the student's full name."));

        return original is null
            ? repository.RegisterAsync(new Student(number, name, contact))
            : repository.UpdateAsync(original, number, name, contact);
    }

    public Task<WriteResult> SetEligibilityAsync(Student? student, bool allowed) =>
        student is null
            ? Task.FromResult(new WriteResult(false, "Select a student to change their eligibility."))
            : repository.SetEligibilityAsync(student.Id, allowed);
}
