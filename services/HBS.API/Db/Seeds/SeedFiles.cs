using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace HBS.API.Db.Seeds;

internal static class SeedFiles
{
    internal static readonly JsonSerializerOptions Json = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    internal static async Task<T> ReadAsync<T>(string path, CancellationToken token = default)
    {
        await using var file = File.OpenRead(path);
        if (path.EndsWith(".gz", StringComparison.Ordinal))
        {
            await using var gzip = new GZipStream(file, CompressionMode.Decompress);
            return await JsonSerializer.DeserializeAsync<T>(gzip, Json, token)
                ?? throw new InvalidDataException($"Empty JSON: {path}");
        }
        return await JsonSerializer.DeserializeAsync<T>(file, Json, token)
            ?? throw new InvalidDataException($"Empty JSON: {path}");
    }

    internal static async Task WriteAsync<T>(string path, T value, CancellationToken token = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + ".tmp";
        try
        {
            await using (var file = File.Create(temporary))
            {
                if (path.EndsWith(".gz", StringComparison.Ordinal))
                {
                    await using var gzip = new GZipStream(file, CompressionLevel.SmallestSize);
                    await JsonSerializer.SerializeAsync(gzip, value, Json, token);
                }
                else
                    await JsonSerializer.SerializeAsync(file, value, new JsonSerializerOptions(Json) { WriteIndented = true }, token);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static async Task<GeographySource> ReadSourceAsync(string root, CancellationToken token)
    {
        var path = Path.Combine(root, "Sources", "geography.json.gz");
        using var manifest = await ReadAsync<JsonDocument>(Path.Combine(root, "Sources", "manifest.json"), token);
        await using var stream = File.OpenRead(path);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, token));
        if (!hash.Equals(manifest.RootElement.GetProperty("SnapshotSha256").GetString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Geography source checksum mismatch.");
        foreach (var baseline in manifest.RootElement.GetProperty("BaselineSha256").EnumerateObject())
        {
            await using var csv = File.OpenRead(Path.Combine(root, baseline.Name));
            var csvHash = Convert.ToHexString(await SHA256.HashDataAsync(csv, token));
            if (!csvHash.Equals(baseline.Value.GetString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Baseline changed: {baseline.Name}; reconcile and regenerate the source snapshot first.");
        }
        return await ReadAsync<GeographySource>(path, token);
    }
}
