using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfStudentRepository(PostgresOperations operations) : IStudentRepository
{
    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        operations.ReadAsync(db => db.Students.AsNoTracking()
            .SingleOrDefaultAsync(student => student.Id == id, cancellationToken));

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        operations.ReadAsync<IReadOnlyList<Student>>(async db => await db.Students.AsNoTracking()
            .OrderBy(student => student.Name).ThenBy(student => student.Id)
            .ToListAsync(cancellationToken));
}
