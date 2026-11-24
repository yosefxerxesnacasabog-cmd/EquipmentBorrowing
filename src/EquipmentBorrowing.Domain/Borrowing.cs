namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; private set; }

    public Student Student { get; private set; } = null!;

    public Equipment Equipment { get; private set; } = null!;

    public int StudentId { get; private set; }

    public int EquipmentId { get; private set; }

    public DateTime DateBorrowed { get; private set; }

    public DateTime ExpectedReturnDate { get; private set; }

    public DateTime? ActualReturnDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    private Borrowing()
    {
    }

    public Borrowing(
        Student student,
        Equipment equipment,
        DateTime dateBorrowed,
        DateTime expectedReturnDate)
    {
        Student = student;
        Equipment = equipment;
        StudentId = student.Id;
        EquipmentId = equipment.Id;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkAsReturned(DateTime? returnDate = null)
    {
        ActualReturnDate = (returnDate ?? DateTime.Today).Date;
        Status = BorrowingStatus.Returned;
        Equipment.MarkAsAvailable();
    }

    public static Borrowing FromSnapshot(int id, Student student, Equipment equipment,
        DateTime borrowedDate, DateTime expectedReturnDate, DateTime? returnedDate, BorrowingStatus status) =>
        new(student, equipment, borrowedDate, expectedReturnDate)
        {
            Id = id,
            ActualReturnDate = returnedDate,
            Status = status
        };
}
