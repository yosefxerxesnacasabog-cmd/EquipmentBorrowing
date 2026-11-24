using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Api;

internal static class WorkflowEndpoints
{
    internal static void MapWorkflowEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapPost("/students", CreateStudentAsync);
        routes.MapPut("/students/{id}", UpdateStudentAsync);
        routes.MapPatch("/students/{id}/eligibility", SetEligibilityAsync);
        routes.MapPost("/equipment", CreateEquipmentAsync);
        routes.MapPut("/equipment/{id}", UpdateEquipmentAsync);
        routes.MapPost("/borrowings", CheckoutAsync);
        routes.MapPost("/checkout-requests", BatchAsync);
        routes.MapPost("/borrowings/{id}/return", ReturnAsync);
        routes.MapGet("/borrowings/{id}", GetBorrowingAsync);
        routes.MapGet("/borrowings", SearchAsync);
        routes.MapGet("/students/{id}/borrowings", StudentHistoryAsync);
        routes.MapGet("/equipment/{id}/borrowings", EquipmentHistoryAsync);
    }

    private static IResult Missing() => Results.Json(ApiErrors.ForStatus(404), statusCode: 404);
    private static IResult Invalid() => Results.Json(ApiErrors.ForStatus(400), statusCode: 400);

    private static IResult Error(WriteResult result)
    {
        var (status, code) = result.Failure switch
        {
            WriteFailure.Missing => (404, "not_found"),
            WriteFailure.Conflict => (409, "conflict"),
            WriteFailure.Server => (500, "database_error"),
            WriteFailure.Unknown => (500, "outcome_unknown"),
            _ => (400, "invalid_request")
        };
        return Results.Json(new ApiError(code, result.Message), statusCode: status);
    }

    private static async Task<IResult> CreateStudentAsync(StudentCreate input, StudentManagementService service)
    {
        var result = await service.SaveAsync(null, input.StudentNumber, input.FullName, input.Contact);
        return result.Succeeded && result.Value is Student student
            ? Results.Created($"/api/v1/students/{student.Id}", StudentResponse.From(student)) : Error(result);
    }

    private static async Task<IResult> UpdateStudentAsync(int id, StudentInput input, CatalogService catalog, StudentManagementService service)
    {
        var student = await catalog.GetStudentAsync(id);
        if (student is null) return Missing();
        var result = await service.SaveAsync(student, input.StudentNumber, input.FullName, input.Contact);
        return result.Succeeded && result.Value is Student updated ? Results.Ok(StudentResponse.From(updated)) : Error(result);
    }

    private static async Task<IResult> SetEligibilityAsync(int id, EligibilityInput input, CatalogService catalog, StudentManagementService service)
    {
        var student = await catalog.GetStudentAsync(id);
        if (student is null) return Missing();
        var result = await service.SetEligibilityAsync(student, input.IsEligible);
        return result.Succeeded && result.Value is Student updated ? Results.Ok(StudentResponse.From(updated)) : Error(result);
    }

    private static async Task<IResult> CreateEquipmentAsync(EquipmentCreate input, EquipmentManagementService service)
    {
        var result = await service.SaveAsync(null, input.EquipmentCode, input.Name, input.Description);
        return result.Succeeded && result.Value is Equipment item
            ? Results.Created($"/api/v1/equipment/{item.Id}", EquipmentResponse.From(item)) : Error(result);
    }

    private static async Task<IResult> UpdateEquipmentAsync(int id, EquipmentInput input, CatalogService catalog, EquipmentManagementService service)
    {
        var item = await catalog.GetEquipmentItemAsync(id);
        if (item is null) return Missing();
        var result = await service.SaveAsync(item, input.EquipmentCode, input.Name, input.Description);
        return result.Succeeded && result.Value is Equipment updated ? Results.Ok(EquipmentResponse.From(updated)) : Error(result);
    }

    private static async Task<IResult> CheckoutAsync(CheckoutInput input, LendingService service)
    {
        var result = await service.CheckoutAsync(input.StudentId, input.EquipmentId, input.ExpectedReturnDate?.ToDateTime(TimeOnly.MinValue));
        if (!result.Succeeded) return Error(result);
        var loan = ((IReadOnlyList<Borrowing>)result.Value!)[0];
        return Results.Created($"/api/v1/borrowings/{loan.Id}", BorrowingResponse.From(loan));
    }

    private static async Task<IResult> BatchAsync(BatchInput input, LendingService service)
    {
        var result = await service.CheckoutMultipleAsync(input.StudentId, input.EquipmentIds ?? [],
            input.ExpectedReturnDate?.ToDateTime(TimeOnly.MinValue));
        return result.Succeeded
            ? Results.Json(new { borrowings = ((IReadOnlyList<Borrowing>)result.Value!).Select(BorrowingResponse.From).ToArray() }, statusCode: 201)
            : Error(result);
    }

    private static async Task<IResult> ReturnAsync(int id, ReturnInput input, LendingService service, BorrowingReviewService review)
    {
        var result = await service.ReturnAsync(id);
        if (!result.Succeeded) return Error(result);
        var loan = await review.GetByIdAsync(id);
        return loan is null ? Missing() : Results.Ok(BorrowingResponse.From(loan));
    }

    private static async Task<IResult> GetBorrowingAsync(int id, BorrowingReviewService review, CancellationToken cancellationToken)
    {
        var loan = await review.GetByIdAsync(id, cancellationToken);
        return loan is null ? Missing() : Results.Ok(BorrowingResponse.From(loan));
    }

    private static async Task<IResult> SearchAsync(string? search, string? status, string? sort, string? direction,
        BorrowingReviewService review, CancellationToken cancellationToken)
    {
        if (status is not (null or "All" or "Active" or "Returned") ||
            sort is not (null or "borrowedDate" or "expectedReturnDate") ||
            direction is not (null or "asc" or "desc")) return Invalid();
        BorrowingStatus? filter = status switch { "Active" => BorrowingStatus.Active, "Returned" => BorrowingStatus.Returned, _ => null };
        var rows = await review.SearchAsync(new(search, filter,
            sort == "expectedReturnDate" ? BorrowingSortField.ExpectedReturnDate : BorrowingSortField.CheckoutDate,
            direction == "desc"), cancellationToken);
        return Results.Ok(rows.Select(BorrowingResponse.From).ToArray());
    }

    private static async Task<IResult> StudentHistoryAsync(int id, BorrowingReviewService review, CancellationToken cancellationToken)
    {
        var rows = await review.GetStudentHistoryAsync(id, cancellationToken);
        return rows is null ? Missing() : Results.Ok(rows.Select(BorrowingResponse.From).ToArray());
    }

    private static async Task<IResult> EquipmentHistoryAsync(int id, BorrowingReviewService review, CancellationToken cancellationToken)
    {
        var rows = await review.GetEquipmentHistoryAsync(id, cancellationToken);
        return rows is null ? Missing() : Results.Ok(rows.Select(BorrowingResponse.From).ToArray());
    }
}
