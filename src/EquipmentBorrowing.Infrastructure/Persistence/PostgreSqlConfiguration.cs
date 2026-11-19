using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class PostgreSqlConfiguration
{
    public const string ConnectionVariable = "EQUIPMENT_BORROWING_CONNECTION";

    public static void Configure(DbContextOptionsBuilder options)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Model inspection and migration generation do not open a connection.
            options.UseNpgsql();
            return;
        }

        options.UseNpgsql(connectionString);
    }
}
