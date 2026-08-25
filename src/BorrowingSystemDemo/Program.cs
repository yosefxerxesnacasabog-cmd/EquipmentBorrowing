using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;

Console.WriteLine("=== CAMPUS EQUIPMENT BORROWING SYSTEM ===");
Console.WriteLine();

var studentRepository = new InMemoryStudentRepository();
var equipmentRepository = new InMemoryEquipmentRepository();
var borrowingRepository = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository);



Console.WriteLine("[TEST 1] Successful Borrowing Case");
Console.WriteLine("Student ID: 1 requests Equipment ID: 1");
Console.WriteLine();

var successResult = await service.BorrowEquipmentAsync(
    1,
    1,
    DateTime.Now.AddDays(7));

if (successResult)
{
    Console.WriteLine("Result: SUCCESS");
    Console.WriteLine("Borrowing record created successfully.");
}
else
{
    Console.WriteLine("Result: FAILED");
}

Console.WriteLine();



Console.WriteLine("[TEST 2] Student Not Allowed Case");
Console.WriteLine("Student ID: 3 requests Equipment ID: 1");
Console.WriteLine();

var failedResult = await service.BorrowEquipmentAsync(
    3,
    1,
    DateTime.Now.AddDays(7));

if (failedResult)
{
    Console.WriteLine("Result: SUCCESS");
}
else
{
    Console.WriteLine("Result: FAILED");
    Console.WriteLine("Student is not allowed to borrow equipment.");
}

Console.WriteLine();

Console.WriteLine("=== DEMONSTRATION COMPLETE ===");