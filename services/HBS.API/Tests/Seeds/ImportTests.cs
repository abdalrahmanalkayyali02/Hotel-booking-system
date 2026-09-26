using System.Text.Json;
using HBS.API.Db.models;
using HBS.API.Db.Seeds;

internal static class ImportTests
{
    public static async Task<int> RunAsync(string root, SeedData fixture)
    {
        var passed = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("Import: " + message);
            passed++;
        }
        Type[] order = [typeof(Languages), typeof(Roles), typeof(Regions), typeof(SubRegions),
            typeof(Countries), typeof(States), typeof(Cities), typeof(RoleTranslation),
            typeof(CountriesTranslation), typeof(StatesTranslation), typeof(CitiesTranslation),
            typeof(RegionsTranslation), typeof(SubRegionsTranslation)];
        async Task<(int Code, string Output, string Error)> Command(Func<ISeedImportDatabase> factory)
        {
            var oldOut = Console.Out;
            var oldError = Console.Error;
            using var output = new StringWriter();
            using var error = new StringWriter();
            try
            {
                Console.SetOut(output);
                Console.SetError(error);
                var code = await SeedCommand.RunAsync(["--seed-import", root], factory);
                return (code, output.ToString(), error.ToString());
            }
            finally { Console.SetOut(oldOut); Console.SetError(oldError); }
        }

        var database = new FakeDatabase();
        var result = await Command(() => database);
        Check(result.Code == 0 && result.Error == "", "Command must recognize --seed-import and succeed.");
        Check(database.Events[0] == "begin" && database.Events[^1] == "dispose", "Transaction/context lifetime.");
        Check(database.Checked.SequenceEqual(order), "All target tables must be checked in dependency order.");
        Check(database.Batches.Select(b => b.Type).SequenceEqual(order), "Insert dependency order.");
        Check(database.Events.IndexOf("insert") > database.Events.LastIndexOf("check"), "Check all tables before inserting any rows.");
        Check(database.Commits == 1 && database.Rollbacks == 0, "Commit once after all collections succeed.");
        var fixtureCount = typeof(SeedData).GetProperties().Sum(p => ((System.Collections.ICollection)p.GetValue(fixture)!).Count);
        Check(database.CommittedRows.Count == fixtureCount, "Success must commit all fixture rows.");
        Check(result.Output.Contains("Seed import committed:") && result.Output.Contains($"{fixtureCount:N0} rows inserted."), "Success summary total.");
        foreach (var p in typeof(SeedData).GetProperties())
            Check(result.Output.Contains($"{p.Name}: {((System.Collections.ICollection)p.GetValue(fixture)!).Count:N0}"), "Summary must include " + p.Name);
        var expectedIds = typeof(SeedData).GetProperties().SelectMany(p => ((System.Collections.IEnumerable)p.GetValue(fixture)!).Cast<object>())
            .Select(Id).Order().ToArray();
        Check(database.CommittedRows.Select(Id).Order().SequenceEqual(expectedIds), "Existing UUIDs must be inserted unchanged.");

        foreach (var occupied in order)
        {
            var nonempty = new FakeDatabase { Occupied = occupied };
            var refusal = await Command(() => nonempty);
            Check(refusal.Code != 0 && refusal.Error.Contains("already contains rows") && refusal.Error.Contains(occupied.Name), "Refuse/report nonempty " + occupied.Name);
            Check(nonempty.Batches.Count == 0 && nonempty.Commits == 0 && nonempty.Rollbacks == 1, "Nonempty refusal must insert nothing and roll back.");
        }
        foreach (var failure in new[] { "begin", "check", "save", "commit", "rollback" })
        {
            var broken = new FakeDatabase { Failure = failure };
            var failed = await Command(() => broken);
            Check(failed.Code != 0 && !failed.Output.Contains("committed:"), "Failure must not print success: " + failure);
            Check(broken.Disposed, "Failure must dispose context: " + failure);
            if (failure != "begin") Check(broken.Rollbacks == 1, "Failure must attempt rollback: " + failure);
            Check(broken.CommittedRows.Count == 0, "Failure must not leave committed rows: " + failure);
            if (failure == "rollback") Check(failed.Error.Contains("rollback could not be confirmed"), "Report rollback failure without claiming success.");
        }

        var path = Path.Combine(root, "Staging/seed-data.json.gz");
        var original = await File.ReadAllBytesAsync(path);
        try
        {
            var invalid = JsonSerializer.Deserialize<SeedData>(JsonSerializer.Serialize(fixture))!;
            invalid.CitiesTranslations.Clear();
            await SeedFiles.WriteAsync(path, invalid);
            var reachedFactory = false;
            var refused = await Command(() => { reachedFactory = true; throw new Exception("DB must not be reached"); });
            Check(refused.Code != 0 && !reachedFactory, "Validation failure must precede context creation and transaction opening.");
            Check(refused.Error.Contains("before database access"), "Validation refusal should explain the failure stage.");
        }
        finally { await File.WriteAllBytesAsync(path, original); }

        // Exercise both large collections across batch boundaries, without a database.
        var large = JsonSerializer.Deserialize<SeedData>(JsonSerializer.Serialize(fixture))!;
        var city = large.Cities[0];
        large.Cities = Enumerable.Range(0, SeedImporter.BatchSize * 2 + 1).Select(_ => new Cities
        {
            Id = Guid.CreateVersion7(), CityName = "Fixture", CountryId = city.CountryId,
            StateId = city.StateId, RegionId = city.RegionId
        }).ToList();
        large.CitiesTranslations = large.Cities.SelectMany(c => large.Languages.Select(l => new CitiesTranslation
        {
            Id = Guid.CreateVersion7(), CityId = c.Id, LanguageId = l.Id, Name = "Fixture"
        })).ToList();
        var batches = new FakeDatabase();
        await SeedImporter.ImportAsync(large, batches);
        Check(batches.Batches.Where(b => b.Type == typeof(Cities)).Select(b => b.Count).SequenceEqual(new[] { 1000, 1000, 1 }), "Cities must use bounded batches.");
        Check(batches.Batches.Where(b => b.Type == typeof(CitiesTranslation)).Select(b => b.Count).SequenceEqual(new[] { 1000, 1000, 1000, 1000, 2 }), "City translations must use bounded batches.");
        Check(batches.Batches.All(b => b.Count <= SeedImporter.BatchSize), "No batch may exceed the configured size.");
        Check(batches.Events.Count(e => e == "begin") == 1 && batches.Commits == 1, "All batches must share one transaction.");
        return passed;
    }

    public static async Task<int> CheckWorldwideAsync(SeedData data)
    {
        var database = new FakeDatabase();
        var counts = await SeedImporter.ImportAsync(data, database);
        if (counts.Values.Sum() != 475685 || database.CommittedRows.Count != 475685)
            throw new Exception("Worldwide import plan must contain every staged row.");
        if (database.Batches.Count != 485 || database.Batches.Any(b => b.Count > 1000))
            throw new Exception("Worldwide import plan must use exactly 485 bounded SaveChanges batches.");
        if (database.Batches.Count(b => b.Type == typeof(Cities)) != 153
            || database.Batches.Count(b => b.Type == typeof(CitiesTranslation)) != 306)
            throw new Exception("Worldwide city batching counts changed.");
        if (database.Commits != 1 || database.Rollbacks != 0)
            throw new Exception("Worldwide batches must commit together.");
        return 4;
    }

    private static Guid Id(object row) => (Guid)row.GetType().GetProperty(row is Regions ? "RegionId" : "Id")!.GetValue(row)!;

    private sealed class FakeDatabase : ISeedImportDatabase
    {
        public Type? Occupied { get; init; }
        public string? Failure { get; init; }
        public List<string> Events { get; } = [];
        public List<Type> Checked { get; } = [];
        public List<(Type Type, int Count)> Batches { get; } = [];
        public List<object> CommittedRows { get; } = [];
        private readonly List<object> _pending = [];
        private bool _active;
        public int Commits { get; private set; }
        public int Rollbacks { get; private set; }
        public bool Disposed { get; private set; }
        public Task BeginAsync()
        {
            Events.Add("begin");
            if (Failure == "begin") throw new IOException("Simulated connection failure");
            _active = true;
            return Task.CompletedTask;
        }
        public Task<bool> HasRowsAsync<T>() where T : class
        {
            if (!_active) throw new Exception("No transaction");
            Events.Add("check"); Checked.Add(typeof(T));
            if (Failure == "check") throw new IOException("Simulated query failure");
            return Task.FromResult(Occupied == typeof(T));
        }
        public Task InsertBatchAsync<T>(T[] rows) where T : class
        {
            if (!_active) throw new Exception("No transaction");
            Events.Add("insert"); Batches.Add((typeof(T), rows.Length));
            if (Failure is "save" or "rollback" && Batches.Count == 3)
                throw new IOException("Simulated SaveChanges failure after successful earlier batches");
            _pending.AddRange(rows);
            return Task.CompletedTask;
        }
        public Task CommitAsync()
        {
            if (!_active) throw new Exception("No transaction");
            Events.Add("commit");
            if (Failure == "commit") throw new IOException("Simulated commit failure");
            CommittedRows.AddRange(_pending); _pending.Clear(); _active = false; Commits++;
            return Task.CompletedTask;
        }
        public Task RollbackAsync()
        {
            Events.Add("rollback"); Rollbacks++;
            if (Failure == "rollback") throw new IOException("Simulated rollback failure");
            _pending.Clear(); _active = false;
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync()
        {
            Events.Add("dispose"); Disposed = true; _pending.Clear(); _active = false;
            return ValueTask.CompletedTask;
        }
    }
}
