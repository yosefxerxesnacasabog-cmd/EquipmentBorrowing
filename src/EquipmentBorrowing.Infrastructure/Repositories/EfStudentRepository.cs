using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfStudentRepository(PostgresOperations operations) : IStudentRepository, IStudentManagementRepository
{
    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        operations.ReadAsync(db => db.Students.AsNoTracking()
            .SingleOrDefaultAsync(student => student.Id == id, cancellationToken));

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        operations.ReadAsync<IReadOnlyList<Student>>(async db => await db.Students.AsNoTracking()
            .OrderBy(student => student.Name).ThenBy(student => student.Id)
            .ToListAsync(cancellationToken));

    public Task<WriteResult> RegisterAsync(Student student) =>
        operations.WriteAsync(db =>
        {
            db.Students.Add(student);
            return Task.FromResult(new WriteResult(true, "Student registered and eligible to borrow.", Value: student));
        });

    public Task<WriteResult> UpdateAsync(Student original, string number, string name, string? contact) =>
        operations.WriteAsync(async db =>
        {
            var current = await db.Students.FindAsync(original.Id);
            if (current is null)
                return new(false, "This student no longer exists.", true, WriteFailure.Missing);
            if (current.StudentNumber != original.StudentNumber || current.Name != original.Name ||
                current.Contact != original.Contact)
                return new(false, "This student's information changed. Review the refreshed record and try again.", true, WriteFailure.Conflict);

            current.UpdateInformation(number, name, contact);
            return new(true, "Student information updated.", Value: current);
        });

    public Task<WriteResult> SetEligibilityAsync(int studentId, bool allowed) =>
        operations.WriteAsync(async db =>
        {
            var student = await db.Students.FindAsync(studentId);
            if (student is null)
                return new(false, "This student no longer exists.", true, WriteFailure.Missing);
            if (student.IsAllowedToBorrow == allowed)
                return new(true, allowed ? "This student is already eligible." : "This student is already restricted.", Value: student);

            student.SetEligibility(allowed);
            return new(true, allowed ? "Student may now borrow." : "Student is restricted from new checkouts.", Value: student);
        });
}
