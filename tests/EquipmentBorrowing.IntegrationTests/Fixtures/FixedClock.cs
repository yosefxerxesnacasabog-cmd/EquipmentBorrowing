namespace EquipmentBorrowing.IntegrationTests.Fixtures;

public sealed class FixedClock : TimeProvider
{
    // D is deliberately unrelated to the machine date. API mutations and seed
    // offsets must use the same local calendar date without changing Windows time.
    public static readonly DateOnly D = new(2026, 11, 10);
    public override DateTimeOffset GetUtcNow() => new(2026, 11, 10, 4, 0, 0, TimeSpan.Zero);
    public override TimeZoneInfo LocalTimeZone { get; } =
        TimeZoneInfo.CreateCustomTimeZone("Lab6-UTC+08", TimeSpan.FromHours(8), "Lab6 UTC+08", "Lab6 UTC+08");
}
