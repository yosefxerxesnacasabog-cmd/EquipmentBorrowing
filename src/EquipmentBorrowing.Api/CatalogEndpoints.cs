using EquipmentBorrowing.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EquipmentBorrowing.Api;

internal static class CatalogEndpoints
{
    internal static void MapCatalogEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/equipment", GetEquipmentAsync).WithName("ListEquipment");
        routes.MapGet("/equipment/{id}", GetEquipmentByIdAsync).WithName("GetEquipment");
        routes.MapGet("/students", GetStudentsAsync).WithName("ListStudents");
        routes.MapGet("/students/{id}", GetStudentByIdAsync).WithName("GetStudent");
    }

    private static async Task<Ok<EquipmentResponse[]>> GetEquipmentAsync(
        bool? available, CatalogService catalog, CancellationToken cancellationToken)
    {
        var items = await catalog.GetEquipmentAsync(available == true, cancellationToken);
        return TypedResults.Ok(items.Select(EquipmentResponse.From).ToArray());
    }

    private static async Task<Results<Ok<EquipmentResponse>, JsonHttpResult<ApiError>>> GetEquipmentByIdAsync(
        int id, CatalogService catalog, CancellationToken cancellationToken)
    {
        var item = await catalog.GetEquipmentItemAsync(id, cancellationToken);
        return item is null
            ? TypedResults.Json(ApiErrors.ForStatus(404), statusCode: 404)
            : TypedResults.Ok(EquipmentResponse.From(item));
    }

    private static async Task<Ok<StudentResponse[]>> GetStudentsAsync(
        string? search, CatalogService catalog, CancellationToken cancellationToken)
    {
        var students = await catalog.GetStudentsAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            students = students.Where(student =>
                student.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                student.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        return TypedResults.Ok(students.Select(StudentResponse.From).ToArray());
    }

    private static async Task<Results<Ok<StudentResponse>, JsonHttpResult<ApiError>>> GetStudentByIdAsync(
        int id, CatalogService catalog, CancellationToken cancellationToken)
    {
        var student = await catalog.GetStudentAsync(id, cancellationToken);
        return student is null
            ? TypedResults.Json(ApiErrors.ForStatus(404), statusCode: 404)
            : TypedResults.Ok(StudentResponse.From(student));
    }
}
