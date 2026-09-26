namespace HBS.API.Db.Seeds;

public sealed class SeedDataLoader
{
    public async Task<SeedData> LoadAsync(string? root = null, CancellationToken cancellationToken = default)
    {
        var (data, issues) = await AuditAsync(root, cancellationToken);
        if (issues.Count != 0) throw new SeedValidationException(issues);
        return data;
    }

    /// <summary>Inspects staging data only. The returned data is not approved for database import.</summary>
    public async Task<(SeedData Data, IReadOnlyList<SeedIssue> Issues)> AuditAsync(string? root = null, CancellationToken cancellationToken = default)
    {
        root ??= Path.Combine(AppContext.BaseDirectory, "Db", "Seeds");
        var source = await SeedFiles.ReadSourceAsync(root, cancellationToken);
        var ids = await SeedFiles.ReadAsync<SortedDictionary<string, Guid>>(Path.Combine(root, "id-map.json.gz"), cancellationToken);
        var corrections = await SeedFiles.ReadAsync<Dictionary<string, TranslationCorrection>>(Path.Combine(root, "Sources", "arabic-corrections.json"), cancellationToken);
        var data = await SeedFiles.ReadAsync<SeedData>(Path.Combine(root, "Staging", "seed-data.json.gz"), cancellationToken);
        return (data, SeedValidator.Validate(data, ids, source, corrections));
    }
}
