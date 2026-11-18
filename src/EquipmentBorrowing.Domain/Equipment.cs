namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Identifier { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; }

    private Equipment() { }

    public Equipment(int id, string name, bool isAvailable = true)
        : this($"EQ-{id:000}", name, null)
    {
        Id = id;
        IsAvailable = isAvailable;
    }

    public Equipment(string identifier, string name, string? description)
    {
        UpdateInformation(identifier, name, description);
        IsAvailable = true;
    }

    public void UpdateInformation(string identifier, string name, string? description)
    {
        Identifier = identifier;
        Name = name;
        Description = description;
    }

    public void MarkAsBorrowed() => IsAvailable = false;
    public void MarkAsAvailable() => IsAvailable = true;
}
