namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; private set; }
    public string StudentNumber { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Contact { get; private set; }
    public bool IsAllowedToBorrow { get; private set; }

    private Student() { }

    public Student(int id, string name, bool isAllowedToBorrow)
        : this($"STU-{id:000}", name, null)
    {
        Id = id;
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public Student(string studentNumber, string name, string? contact)
    {
        UpdateInformation(studentNumber, name, contact);
        IsAllowedToBorrow = true;
    }

    public void UpdateInformation(string studentNumber, string name, string? contact)
    {
        StudentNumber = studentNumber;
        Name = name;
        Contact = contact;
    }

    public void SetEligibility(bool allowed) => IsAllowedToBorrow = allowed;
}
