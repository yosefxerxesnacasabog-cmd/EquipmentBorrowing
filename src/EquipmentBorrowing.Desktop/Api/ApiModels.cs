using System;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.Api;

internal sealed record StudentJson(int Id, string StudentNumber, string FullName, string? Contact, bool IsEligible)
{
    public Student ToModel()
    {
        var student = new Student(Id, FullName, IsEligible);
        student.UpdateInformation(StudentNumber, FullName, Contact);
        return student;
    }
}
internal sealed record EquipmentJson(int Id, string EquipmentCode, string Name, string? Description, bool IsAvailable)
{
    public Equipment ToModel()
    {
        var item = new Equipment(Id, Name, IsAvailable);
        item.UpdateInformation(EquipmentCode, Name, Description);
        return item;
    }
}
internal sealed record BorrowingJson(int Id, int StudentId, int EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, StudentJson Student, EquipmentJson Equipment)
{
    public Borrowing ToModel() => Borrowing.FromSnapshot(Id, Student.ToModel(), Equipment.ToModel(),
        BorrowedDate.ToDateTime(TimeOnly.MinValue), ExpectedReturnDate.ToDateTime(TimeOnly.MinValue),
        ReturnedDate?.ToDateTime(TimeOnly.MinValue), Enum.Parse<BorrowingStatus>(Status));
}
