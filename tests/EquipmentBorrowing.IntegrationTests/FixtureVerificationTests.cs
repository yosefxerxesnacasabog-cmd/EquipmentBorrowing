using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EquipmentBorrowing.IntegrationTests.Fixtures;

namespace EquipmentBorrowing.IntegrationTests;

// Infrastructure verification, not a substitute for the 22 TC6 objectives.
public sealed class FixtureVerificationTests
{
    [Fact]
    public async Task Migrated_fixture_exposes_deterministic_related_records_through_real_pipeline()
    {
        await using var scenario = await Scenario.CreateAsync();
        var before = await scenario.SnapshotAsync();
        using var response = await scenario.Client.GetAsync("/api/v1/equipment");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(7, items.GetArrayLength());
        Assert.Equal(4, before.GetProperty("students").GetArrayLength());
        Assert.Equal(4, before.GetProperty("borrowings").GetArrayLength());
        var camera = items.EnumerateArray().Single(x => x.GetProperty("equipmentCode").GetString() == "EQ-001");
        var cameraId = camera.GetProperty("id").GetInt32();
        var history = await scenario.Client.GetFromJsonAsync<JsonElement>($"/api/v1/equipment/{cameraId}/borrowings");
        var loan = Assert.Single(history.EnumerateArray());
        Assert.Equal("ST-100", loan.GetProperty("student").GetProperty("studentNumber").GetString());
        Assert.Equal(FixedClock.D.AddDays(-9).ToString("yyyy-MM-dd"), loan.GetProperty("borrowedDate").GetString());
        Assert.Equal(FixedClock.D.AddDays(-1).ToString("yyyy-MM-dd"), loan.GetProperty("expectedReturnDate").GetString());
        Assert.Equal(before.GetRawText(), (await scenario.SnapshotAsync()).GetRawText());
    }

    [Fact]
    public async Task Independent_scenarios_do_not_share_rows_and_reset_restores_the_same_fixture()
    {
        await using var first = await Scenario.CreateAsync();
        await using var second = await Scenario.CreateAsync();
        Assert.NotEqual(first.DatabaseName, second.DatabaseName);
        var initial = await first.SnapshotAsync();
        var isolated = await second.SnapshotAsync();
        using var response = await first.Client.PostAsJsonAsync("/api/v1/students",
            new { studentNumber = "ISOLATION-001", fullName = "Fixture Isolation", contact = (string?)null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(5, (await first.SnapshotAsync()).GetProperty("students").GetArrayLength());
        Assert.Equal(isolated.GetRawText(), (await second.SnapshotAsync()).GetRawText());
        await first.ResetAsync();
        Assert.Equal(initial.GetRawText(), (await first.SnapshotAsync()).GetRawText());
    }

    [Fact]
    public async Task Checkout_validation_and_persisted_dates_use_injected_clock_instead_of_machine_date()
    {
        await using var scenario = await Scenario.CreateAsync();
        var students = await scenario.Client.GetFromJsonAsync<JsonElement>("/api/v1/students");
        var items = await scenario.Client.GetFromJsonAsync<JsonElement>("/api/v1/equipment");
        var studentId = students.EnumerateArray().Single(x => x.GetProperty("studentNumber").GetString() == "ST-400").GetProperty("id").GetInt32();
        var itemId = items.EnumerateArray().Single(x => x.GetProperty("equipmentCode").GetString() == "EQ-002").GetProperty("id").GetInt32();
        var before = await scenario.SnapshotAsync();
        using var invalid = await scenario.Client.PostAsJsonAsync("/api/v1/borrowings",
            new { studentId, equipmentId = itemId, expectedReturnDate = FixedClock.D });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(before.GetRawText(), (await scenario.SnapshotAsync()).GetRawText());
        using var response = await scenario.Client.PostAsJsonAsync("/api/v1/borrowings",
            new { studentId, equipmentId = itemId, expectedReturnDate = FixedClock.D.AddDays(2) });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var loan = await response.Content.ReadFromJsonAsync<JsonElement>();
        var loanId = loan.GetProperty("id").GetInt32();
        Assert.Equal(FixedClock.D.ToString("yyyy-MM-dd"), loan.GetProperty("borrowedDate").GetString());
        var stored = Assert.Single((await scenario.SnapshotAsync()).GetProperty("borrowings").EnumerateArray(),
            x => x.GetProperty("Id").GetInt32() == loanId);
        Assert.Equal(FixedClock.D.ToString("yyyy-MM-dd"), stored.GetProperty("DateBorrowed").GetString());
        using var returned = await scenario.Client.PostAsJsonAsync($"/api/v1/borrowings/{loanId}/return", new { });
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        var fresh = Assert.Single((await scenario.SnapshotAsync()).GetProperty("borrowings").EnumerateArray(),
            x => x.GetProperty("Id").GetInt32() == loanId);
        Assert.Equal("Returned", fresh.GetProperty("Status").GetString());
        Assert.Equal(FixedClock.D.ToString("yyyy-MM-dd"), fresh.GetProperty("ActualReturnDate").GetString());
    }
}
