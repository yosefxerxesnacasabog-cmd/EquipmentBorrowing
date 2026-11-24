using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.Api;

internal sealed class HttpStudents(ApiTransport api) : IStudentRepository, IStudentManagementRepository
{
    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await api.ReadAsync<StudentJson>($"students/{id}", cancellationToken, true))?.ToModel();
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await api.ReadAsync<StudentJson[]>("students", cancellationToken))!.Select(row => row.ToModel()).ToArray();
    public Task<WriteResult> RegisterAsync(Student student) =>
        api.WriteAsync(HttpMethod.Post, "students", new { studentNumber = student.StudentNumber, fullName = student.Name, contact = student.Contact });
    public Task<WriteResult> UpdateAsync(Student original, string number, string name, string? contact) =>
        api.WriteAsync(HttpMethod.Put, $"students/{original.Id}", new { studentNumber = number, fullName = name, contact });
    public Task<WriteResult> SetEligibilityAsync(int studentId, bool allowed) =>
        api.WriteAsync(HttpMethod.Patch, $"students/{studentId}/eligibility", new { isEligible = allowed });
}

internal sealed class HttpEquipment(ApiTransport api) : IEquipmentRepository, IEquipmentManagementRepository
{
    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await api.ReadAsync<EquipmentJson>($"equipment/{id}", cancellationToken, true))?.ToModel();
    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await api.ReadAsync<EquipmentJson[]>("equipment", cancellationToken))!.Select(row => row.ToModel()).ToArray();
    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default) =>
        (await api.ReadAsync<EquipmentJson[]>("equipment?available=true", cancellationToken))!.Select(row => row.ToModel()).ToArray();
    public Task<WriteResult> RegisterAsync(Equipment equipment) =>
        api.WriteAsync(HttpMethod.Post, "equipment", new { equipmentCode = equipment.Identifier, name = equipment.Name, description = equipment.Description });
    public Task<WriteResult> UpdateAsync(Equipment original, string identifier, string name, string? description) =>
        api.WriteAsync(HttpMethod.Put, $"equipment/{original.Id}", new { equipmentCode = identifier, name, description });
}

internal sealed class HttpBorrowings(ApiTransport api) : ILendingRepository, IBorrowingReviewRepository
{
    public Task<IReadOnlyList<Borrowing>> GetActiveAsync() =>
        SearchAsync(new BorrowingSearch(Status: BorrowingStatus.Active), default);
    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        (await api.ReadAsync<BorrowingJson>($"borrowings/{id}", cancellationToken, true))?.ToModel();
    public async Task<IReadOnlyList<Borrowing>> SearchAsync(BorrowingSearch search, CancellationToken cancellationToken)
    {
        var route = "borrowings?search=" + Uri.EscapeDataString(search.Text ?? "") +
            "&status=" + (search.Status?.ToString() ?? "All") +
            "&sort=" + (search.SortField == BorrowingSortField.CheckoutDate ? "borrowedDate" : "expectedReturnDate") +
            "&direction=" + (search.Descending ? "desc" : "asc");
        return (await api.ReadAsync<BorrowingJson[]>(route, cancellationToken))!.Select(row => row.ToModel()).ToArray();
    }
    public async Task<IReadOnlyList<Borrowing>?> GetStudentHistoryAsync(int studentId, CancellationToken cancellationToken) =>
        (await api.ReadAsync<BorrowingJson[]>($"students/{studentId}/borrowings", cancellationToken, true))?.Select(row => row.ToModel()).ToArray();
    public async Task<IReadOnlyList<Borrowing>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken) =>
        (await api.ReadAsync<BorrowingJson[]>($"equipment/{equipmentId}/borrowings", cancellationToken, true))?.Select(row => row.ToModel()).ToArray();
    public Task<WriteResult> CheckoutAsync(int studentId, IReadOnlyList<int> equipmentIds, DateTime expectedReturnDate) =>
        equipmentIds.Count == 1
            ? api.WriteAsync(HttpMethod.Post, "borrowings", new { studentId, equipmentId = equipmentIds[0], expectedReturnDate = DateOnly.FromDateTime(expectedReturnDate) })
            : api.WriteAsync(HttpMethod.Post, "checkout-requests", new { studentId, equipmentIds, expectedReturnDate = DateOnly.FromDateTime(expectedReturnDate) });
    public Task<WriteResult> ReturnAsync(int borrowingId) =>
        api.WriteAsync(HttpMethod.Post, $"borrowings/{borrowingId}/return", new { });
}
