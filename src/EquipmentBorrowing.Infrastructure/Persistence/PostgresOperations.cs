using System.Data;
using EquipmentBorrowing.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class PostgresOperations(IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
{
    private const string MissingConnection = "Set the EQUIPMENT_BORROWING_CONNECTION environment variable and reopen the app.";

    internal async Task<T> ReadAsync<T>(Func<EquipmentBorrowingDbContext, Task<T>> read)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(db.Database.GetConnectionString()))
                throw new StorageException(MissingConnection);

            return await read(db);
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException or ArgumentException)
        {
            throw new StorageException("Could not load records from PostgreSQL. Check the connection and retry.");
        }
    }

    internal async Task<WriteResult> WriteAsync(Func<EquipmentBorrowingDbContext, Task<WriteResult>> write)
    {
        var committing = false;
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(db.Database.GetConnectionString()))
                return new(false, MissingConnection);
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var result = await write(db);
            if (!result.Succeeded)
                return result;

            await db.SaveChangesAsync();
            committing = true;
            await transaction.CommitAsync();
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres)
        {
            return DatabaseRejection(postgres);
        }
        catch (PostgresException exception) when (
            !committing || exception.SqlState is PostgresErrorCodes.SerializationFailure
                or PostgresErrorCodes.DeadlockDetected or PostgresErrorCodes.UniqueViolation
                or PostgresErrorCodes.CheckViolation or PostgresErrorCodes.ForeignKeyViolation)
        {
            return DatabaseRejection(exception);
        }
        catch (Exception exception) when (exception is DbUpdateException or NpgsqlException or TimeoutException or ArgumentException)
        {
            return committing
                ? new(false, "The commit outcome is unknown. Reconnect and check the current records before retrying.", true)
                : new(false, "PostgreSQL could not save the changes. Check the connection and retry.");
        }
    }

    private static WriteResult Conflict() =>
        new(false, "Another operation changed these records. Review the refreshed data and try again.", true);

    private static WriteResult DatabaseRejection(PostgresException exception) => exception.SqlState switch
    {
        PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => Conflict(),
        PostgresErrorCodes.UniqueViolation when exception.ConstraintName == "IX_Borrowings_ActiveEquipment" =>
            new(false, "An item already has an active borrowing. Review the refreshed availability.", true),
        PostgresErrorCodes.UniqueViolation when exception.ConstraintName == "IX_Students_NormalizedStudentNumber" =>
            new(false, "That institutional student number is already registered."),
        PostgresErrorCodes.UniqueViolation when exception.ConstraintName == "IX_Equipment_NormalizedIdentifier" =>
            new(false, "That equipment identifier is already registered."),
        _ => new(false, "PostgreSQL rejected the changes. Review the record and try again.")
    };
}
