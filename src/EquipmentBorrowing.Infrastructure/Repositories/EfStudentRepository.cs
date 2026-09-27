using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;

    public EfStudentRepository(EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Student?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .FirstOrDefaultAsync(
                student => student.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}