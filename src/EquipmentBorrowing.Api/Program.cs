using EquipmentBorrowing.Api;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<EquipmentBorrowingDbContext>(PostgreSqlConfiguration.Configure);
builder.Services.AddSingleton<PostgresOperations>();
builder.Services.AddSingleton<IStudentRepository, EfStudentRepository>();
builder.Services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
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
app.Run();
