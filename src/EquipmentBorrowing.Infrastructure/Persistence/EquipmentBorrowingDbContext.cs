using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContext : DbContext
{
    public EquipmentBorrowingDbContext(DbContextOptions<EquipmentBorrowingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EquipmentBorrowingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
