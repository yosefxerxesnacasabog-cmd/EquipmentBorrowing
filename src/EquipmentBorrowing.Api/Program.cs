using EquipmentBorrowing.Api;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContextFactory<EquipmentBorrowingDbContext>(PostgreSqlConfiguration.Configure);
builder.Services.AddSingleton<PostgresOperations>();
builder.Services.AddSingleton<IStudentRepository, EfStudentRepository>();
builder.Services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
builder.Services.AddSingleton<IStudentManagementRepository, EfStudentRepository>();
builder.Services.AddSingleton<IEquipmentManagementRepository, EfEquipmentRepository>();
builder.Services.AddSingleton<ILendingRepository, EfBorrowingRepository>();
builder.Services.AddSingleton<IBorrowingReviewRepository, EfBorrowingRepository>();
builder.Services.AddSingleton<StudentManagementService>();
builder.Services.AddSingleton<EquipmentManagementService>();
builder.Services.AddSingleton<LendingService>();
builder.Services.AddSingleton<BorrowingReviewService>();
builder.Services.AddSingleton<CatalogService>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(ApiErrors.HandleExceptionAsync));
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    await response.WriteAsJsonAsync(ApiErrors.ForStatus(response.StatusCode));
});

app.MapGroup("/api/v1").MapCatalogEndpoints();
app.MapGroup("/api/v1").MapWorkflowEndpoints();
app.MapGet("/openapi/v1.json", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "openapi.json"), "application/json"));
app.Run();

// Expose the entry point to the integration test host without duplicating routes.
public partial class Program;
