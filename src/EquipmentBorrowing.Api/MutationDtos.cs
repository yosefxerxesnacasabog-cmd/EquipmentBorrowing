using System.Text.Json.Serialization;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Api;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record StudentInput(string? StudentNumber, string? FullName, [property: JsonRequired] string? Contact);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record EquipmentInput(string? EquipmentCode, string? Name, [property: JsonRequired] string? Description);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record EligibilityInput([property: JsonRequired] bool IsEligible);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CheckoutInput(int? StudentId, int? EquipmentId, DateOnly? ExpectedReturnDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record BatchInput(int? StudentId, int[]? EquipmentIds, DateOnly? ExpectedReturnDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ReturnInput;
internal sealed record BorrowingResponse(int Id, int StudentId, int EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, StudentResponse Student, EquipmentResponse Equipment)
{
    internal static BorrowingResponse From(Borrowing loan) => new(loan.Id, loan.StudentId, loan.EquipmentId,
        DateOnly.FromDateTime(loan.DateBorrowed), DateOnly.FromDateTime(loan.ExpectedReturnDate),
        loan.ActualReturnDate is DateTime date ? DateOnly.FromDateTime(date) : null,
        loan.Status.ToString(), StudentResponse.From(loan.Student), EquipmentResponse.From(loan.Equipment));
}
