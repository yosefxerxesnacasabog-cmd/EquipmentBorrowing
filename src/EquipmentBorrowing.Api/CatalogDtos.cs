using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Api;

internal sealed record StudentResponse(int Id, string StudentNumber, string FullName, string? Contact, bool IsEligible)
{
    internal static StudentResponse From(Student student) =>
        new(student.Id, student.StudentNumber, student.Name, student.Contact, student.IsAllowedToBorrow);
}

internal sealed record EquipmentResponse(int Id, string EquipmentCode, string Name, string? Description, bool IsAvailable)
{
    internal static EquipmentResponse From(Equipment item) =>
        new(item.Id, item.Identifier, item.Name, item.Description, item.IsAvailable);
}
