namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; private set; }

    public Student Student { get; private set; }

    public Equipment Equipment { get; private set; }

    public int StudentId { get; private set; }

    public int EquipmentId { get; private set; }

    public DateTime DateBorrowed { get; private set; }

    public DateTime ExpectedReturnDate { get; private set; }

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

    public void MarkAsReturned()
    {
        Status = BorrowingStatus.Returned;
        Equipment.MarkAsAvailable();
    }
}