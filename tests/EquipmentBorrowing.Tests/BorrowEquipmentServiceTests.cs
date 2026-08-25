using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    [Fact]
    public async Task BorrowEquipmentAsync_WithAvailableEquipment_ReturnsTrue()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            1,
            1,
            DateTime.Now.AddDays(7));

        Assert.True(result);
    }
    [Fact]
    public async Task BorrowEquipmentAsync_WhenStudentIsNotAllowed_ReturnsFalse()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            3,
            1,
            DateTime.Now.AddDays(7));

        Assert.False(result);
    }
    [Fact]
    public async Task BorrowEquipmentAsync_WhenEquipmentIsUnavailable_ReturnsFalse()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            1,
            3,
            DateTime.Now.AddDays(7));

        Assert.False(result);
    }
    [Fact]
    public async Task BorrowEquipmentAsync_WhenEquipmentDoesNotExist_ReturnsFalse()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            1,
            99,
            DateTime.Now.AddDays(7));

        Assert.False(result);
    }
    [Fact]
    public async Task BorrowEquipmentAsync_WhenStudentHasThreeActiveBorrowings_ReturnsFalse()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var student = await studentRepository.GetByIdAsync(1);
        var equipment = await equipmentRepository.GetByIdAsync(1);

        var secondEquipment = await equipmentRepository.GetByIdAsync(2);

        await borrowingRepository.AddAsync(
            new EquipmentBorrowing.Domain.Borrowing(
                student!,
                equipment!,
                DateTime.Now,
                DateTime.Now.AddDays(7)));

        await borrowingRepository.AddAsync(
            new EquipmentBorrowing.Domain.Borrowing(
                student!,
                secondEquipment!,
                DateTime.Now,
                DateTime.Now.AddDays(7)));

        await borrowingRepository.AddAsync(
            new EquipmentBorrowing.Domain.Borrowing(
                student!,
                new EquipmentBorrowing.Domain.Equipment(4, "Tablet"),
                DateTime.Now,
                DateTime.Now.AddDays(7)));

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            1,
            1,
            DateTime.Now.AddDays(7));

        Assert.False(result);
    }
    [Fact]
    public async Task BorrowEquipmentAsync_WhenStudentDoesNotExist_ReturnsFalse()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        var result = await service.BorrowEquipmentAsync(
            99,
            1,
            DateTime.Now.AddDays(7));

        Assert.False(result);
    }
    [Fact]
    public async Task DemonstrateSuccessfulBorrowing()
    {
        var studentRepository = new InMemoryStudentRepository();
        var equipmentRepository = new InMemoryEquipmentRepository();
        var borrowingRepository = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(
            studentRepository,
            equipmentRepository,
            borrowingRepository);

        Console.WriteLine("=== Campus Equipment Borrowing System ===");
        Console.WriteLine("Student requests available equipment...");
        Console.WriteLine("Application service validates request...");
        Console.WriteLine("Repositories provide required information...");

        var result = await service.BorrowEquipmentAsync(
            1,
            1,
            DateTime.Now.AddDays(7));

        if (result)
        {
            Console.WriteLine("Borrowing created.");
            Console.WriteLine("Operation succeeded!");
        }
        else
        {
            Console.WriteLine("Operation failed.");
        }

        Assert.True(result);
    }
}
