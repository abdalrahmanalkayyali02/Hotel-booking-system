using System.Text.Json;

namespace HBS.API.Db.Seeds;

public static class SeedCommand
{
    public static async Task<int> RunAsync(string[] args, Func<ISeedImportDatabase>? createImportDatabase = null)
    {
        if (args.Length is 0 or > 2 || args[0] is not ("--seed-generate" or "--seed-validate" or "--seed-import"))
        {
            Console.Error.WriteLine("Usage: HBS.API --seed-generate|--seed-validate|--seed-import [seed-directory]");
            return 2;
        }
        try
        {
            var root = args.Length == 2 ? Path.GetFullPath(args[1]) : Path.Combine(AppContext.BaseDirectory, "Db", "Seeds");
            if (args[0] == "--seed-generate" && args.Length != 2)
                throw new InvalidDataException("Generation requires an explicit source seed directory to avoid modifying build output accidentally.");
            if (args[0] == "--seed-import")
            {
                // Validation must finish before context creation, server discovery or transaction opening.
                var data = await new SeedDataLoader().LoadAsync(root);
                if (createImportDatabase is null)
                    throw new InvalidOperationException("Seed import database factory is not configured.");
                IReadOnlyDictionary<string, int> counts;
                await using (var database = createImportDatabase())
                    counts = await SeedImporter.ImportAsync(data, database);
                Console.WriteLine($"Seed import committed: {counts.Values.Sum():N0} rows inserted.");
                foreach (var (collection, count) in counts)
                    Console.WriteLine($"  {collection}: {count:N0}");
                return 0;
            }
            var issues = args[0] == "--seed-generate"
                ? await SeedGenerator.GenerateAsync(root)
                : (await new SeedDataLoader().AuditAsync(root)).Issues;
            if (issues.Count == 0)
            {
                Console.WriteLine("Seed validation passed: UUIDv7 identities, relationships and required source-language coverage are complete.");
                return 0;
            }
            Console.Error.WriteLine($"Seed validation incomplete: {issues.Count} issue(s).");
            foreach (var group in issues.GroupBy(i => (i.Code, i.Entity)))
                Console.Error.WriteLine($"  {group.Key.Code} / {group.Key.Entity}: {group.Count()}");
            foreach (var issue in issues.Take(5)) Console.Error.WriteLine($"  {issue.Entity}/{issue.SourceId}: {issue.Message}");
            Console.Error.WriteLine("Resolve sourced Arabic names in Sources/arabic-corrections.json and regenerate. See coverage-report.json.");
            return 1;
        }
        catch (SeedValidationException ex)
        {
            Console.Error.WriteLine($"Seed import refused before database access: {ex.Message}");
            return 1;
        }
        catch (Exception ex) when (args[0] == "--seed-import")
        {
            Console.Error.WriteLine($"Seed import failed: {ex.Message}");
            if (ex.GetBaseException() is { } cause && !ReferenceEquals(cause, ex))
                Console.Error.WriteLine($"  Cause: {cause.Message}");
            return 1;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException)
        {
            Console.Error.WriteLine($"Seed operation failed: {ex.Message}");
            return 2;
        }
    }
}
