namespace HBS.API.Db.Seeds;

// The database boundary also allows command/transaction tests without connecting to MySQL.
public interface ISeedImportDatabase : IAsyncDisposable
{
    Task BeginAsync();
    Task<bool> HasRowsAsync<T>() where T : class;
    Task InsertBatchAsync<T>(T[] rows) where T : class;
    Task CommitAsync();
    Task RollbackAsync();
}

public static class SeedImporter
{
    public const int BatchSize = 1000;

    public static async Task<IReadOnlyDictionary<string, int>> ImportAsync(
        SeedData data, ISeedImportDatabase database)
    {
        var tables = new List<(string Name, int Count, Func<Task<bool>> HasRows, Func<Task> Insert)>();
        void Table<T>(string name, List<T> rows) where T : class
        {
            tables.Add((name, rows.Count, () => database.HasRowsAsync<T>(), async () =>
            {
                foreach (var batch in rows.Chunk(BatchSize))
                    await database.InsertBatchAsync(batch);
            }
            ));
        }
        Table(nameof(data.Languages), data.Languages);
        Table(nameof(data.Roles), data.Roles);
        Table(nameof(data.Regions), data.Regions);
        Table(nameof(data.SubRegions), data.SubRegions);
        Table(nameof(data.Countries), data.Countries);
        Table(nameof(data.States), data.States);
        Table(nameof(data.Cities), data.Cities);
        Table(nameof(data.RoleTranslations), data.RoleTranslations);
        Table(nameof(data.CountriesTranslations), data.CountriesTranslations);
        Table(nameof(data.StatesTranslations), data.StatesTranslations);
        Table(nameof(data.CitiesTranslations), data.CitiesTranslations);
        Table(nameof(data.RegionsTranslations), data.RegionsTranslations);
        Table(nameof(data.SubRegionsTranslations), data.SubRegionsTranslations);

        await database.BeginAsync();
        try
        {
            // Check every target, including collections with no staged rows, before any insert.
            foreach (var table in tables)
                if (await table.HasRows())
                    throw new InvalidOperationException($"Seed import refused: target {table.Name} already contains rows. No rows were imported.");
            foreach (var table in tables)
                await table.Insert();
            await database.CommitAsync();
            return tables.ToDictionary(t => t.Name, t => t.Count);
        }
        catch (Exception failure)
        {
            try { await database.RollbackAsync(); }
            catch (Exception rollbackFailure)
            {
                throw new AggregateException("Seed import failed and rollback could not be confirmed. Check database state before retrying.", failure, rollbackFailure);
            }
            throw;
        }
    }
}
