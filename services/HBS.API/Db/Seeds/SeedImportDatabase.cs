using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HBS.API.Db.Seeds;

public sealed class SeedImportDatabase(AppDbContext context) : ISeedImportDatabase
{
    private IDbContextTransaction? _transaction;

    // Invoked only after SeedDataLoader.LoadAsync has successfully validated staging.
    public static ISeedImportDatabase Create()
    {
        // Same default configuration providers, environment and DefaultConnection as Program.cs.
        // Seed CLI switches are not application configuration arguments.
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).Options;
        return new SeedImportDatabase(new AppDbContext(options));
    }

    public async Task BeginAsync()
    {
        // Serializable empty-table reads protect the check/insert window on InnoDB.
        _transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    }

    public async Task<bool> HasRowsAsync<T>() where T : class
    {
        var table = context.Model.FindEntityType(typeof(T))!.GetTableName()!;
        var engine = await context.Database.SqlQuery<string>(
            $"SELECT ENGINE AS `Value` FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {table}")
            .FirstOrDefaultAsync();
        if (!string.Equals(engine, "InnoDB", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Seed import refused: table {table} must exist and use InnoDB for transactional import.");
        return await context.Set<T>().IgnoreQueryFilters().AsNoTracking().AnyAsync();
    }

    public async Task InsertBatchAsync<T>(T[] rows) where T : class
    {
        // FK scalars and existing UUIDs come directly from staging. Parent batches are detached.
        context.Set<T>().AddRange(rows);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    public Task CommitAsync() => _transaction!.CommitAsync();
    public Task RollbackAsync() => _transaction!.RollbackAsync();

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_transaction is not null) await _transaction.DisposeAsync();
        }
        finally { await context.DisposeAsync(); }
    }
}
