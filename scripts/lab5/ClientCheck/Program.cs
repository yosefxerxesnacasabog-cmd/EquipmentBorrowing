using EquipmentBorrowing.Desktop.Api;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

static void Check(bool success, string name) {
    if (!success) throw new Exception(name);
    Console.WriteLine("PASS " + name);
}
var api = new ApiTransport(new HttpClient { BaseAddress = new("http://127.0.0.1:5185/api/v1/") });
var students = new HttpStudents(api);
var items = new HttpEquipment(api);
var loans = new HttpBorrowings(api);
var studentService = new StudentManagementService(students);
var equipmentService = new EquipmentManagementService(items);
var lending = new LendingService(loans);
Check((await studentService.SaveAsync(null, "CLIENT-001", "Client Student", null)).Succeeded, "HTTP student registration");
var student = (await students.GetAllAsync()).Single(s => s.StudentNumber == "CLIENT-001");
Check((await studentService.SaveAsync(student, "CLIENT-001", "Client Edited", null)).Succeeded, "HTTP student update");
Check((await equipmentService.SaveAsync(null, "CLIENT-EQ", "Client Item", null)).Succeeded, "HTTP equipment registration");
var item = (await items.GetAllAsync()).Single(e => e.Identifier == "CLIENT-EQ");
Check((await lending.CheckoutAsync(student.Id, item.Id, DateTime.Today.AddDays(5))).Succeeded, "HTTP checkout");
var loan = (await loans.GetActiveAsync()).Single(l => l.StudentId == student.Id);
Check(loan.Id > 0 && loan.Student.Name == "Client Edited" && loan.Equipment.Id == item.Id, "related HTTP records map correctly");
Check((await lending.ReturnAsync(loan.Id)).Succeeded, "HTTP return");
var history = (await loans.GetStudentHistoryAsync(student.Id, default))!;
Check(history.Single().Id == loan.Id && history[0].ActualReturnDate == DateTime.Today && history[0].Status == BorrowingStatus.Returned, "HTTP returned history and stable identity");
var inactive = new ApiTransport(new HttpClient { BaseAddress = new("http://127.0.0.1:5184/api/v1/"), Timeout = TimeSpan.FromSeconds(2) });
try { await inactive.ReadAsync<StudentJson[]>("students"); throw new Exception("Expected connection failure"); }
catch (StorageException e) { Check((e.Message.Contains("Cannot reach") || e.Message.Contains("timed out")), "unreachable read message"); }
var failed = await inactive.WriteAsync(HttpMethod.Post, "students", new { });
Check(failed.Failure == WriteFailure.Unknown && failed.RefreshRequired, "lost mutation result requires readback");
Check((await inactive.WriteAsync(HttpMethod.Post, "students", new { })).Message.Contains("Refresh"), "mutation retry blocked pending readback");

// A deterministic lost-acknowledgement response exercises the retry gate.
// This simulates the transport contract; it does not pretend to drop a real commit.
var scripted = new ScriptedHandler();
var guarded = new ApiTransport(new HttpClient(scripted) { BaseAddress = new("http://example.invalid/api/v1/") });
Check((await guarded.WriteAsync(HttpMethod.Post, "students", new { })).Failure == WriteFailure.Unknown, "server reports unknown commit outcome");
Check((await guarded.WriteAsync(HttpMethod.Post, "students", new { })).Failure == WriteFailure.Unknown && scripted.Writes == 1, "no automatic or manual repeat before fresh read");
await guarded.ReadAsync<StudentJson[]>("students");
Check((await guarded.WriteAsync(HttpMethod.Post, "students", new { })).Succeeded && scripted.Writes == 2, "fresh read permits staff-directed retry");
var serverError = new ApiTransport(new HttpClient(new ServerErrorHandler()) { BaseAddress = new("http://example.invalid/api/v1/") });
try { await serverError.ReadAsync<StudentJson[]>("students"); throw new Exception("Expected server failure"); }
catch (StorageException error) { Check(error.Message == "Received server failure.", "received HTTP 500 remains distinct from no HTTP response"); }

sealed class ScriptedHandler : HttpMessageHandler
{
    public int Writes { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        if (request.Method == HttpMethod.Get)
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json") });
        Writes++;
        return Task.FromResult(Writes == 1
            ? new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent("{\"code\":\"outcome_unknown\",\"message\":\"Read back before retry.\"}", System.Text.Encoding.UTF8, "application/json") }
            : new HttpResponseMessage(System.Net.HttpStatusCode.Created));
    }
}

sealed class ServerErrorHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
        Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent("{\"code\":\"server_error\",\"message\":\"Received server failure.\"}", System.Text.Encoding.UTF8, "application/json") });
}
