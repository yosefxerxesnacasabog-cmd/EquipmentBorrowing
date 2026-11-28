using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EquipmentBorrowing.IntegrationTests.Fixtures;

public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace the production factory as a whole. Never mutate the process
            // connection environment while other scenarios may be running.
            services.RemoveAll<IDbContextFactory<EquipmentBorrowingDbContext>>();
            services.RemoveAll<EquipmentBorrowingDbContext>();
            services.AddSingleton<IDbContextFactory<EquipmentBorrowingDbContext>>(
                new FreshContextFactory(connectionString));
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedClock());
        });
    }
}

public sealed class FreshContextFactory(string connectionString) : IDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<EquipmentBorrowingDbContext>().UseNpgsql(connectionString).Options);
}
