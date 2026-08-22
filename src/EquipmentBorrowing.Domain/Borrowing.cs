namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public Student Student { get; }
    public Equipment Equipment { get; }
    public DateTime DateBorrowed { get; }
    public DateTime ExpectedReturnDate { get; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        Student student,
        Equipment equipment,
        DateTime dateBorrowed,
        DateTime expectedReturnDate)
    {
        Student = student;
        Equipment = equipment;
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