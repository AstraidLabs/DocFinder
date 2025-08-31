using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain.Settings;

namespace DocFinder.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public SettingsService(string? filePath = null)
    {
        // Store user settings per user in the local application data folder.
        _filePath = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocFinder", "settings.json");
    }

    public AppSettings Current { get; private set; } = new();

    public async Task<AppSettings> LoadAsync(CancellationToken ct = default)
    {
        AppSettings? loaded = null;
        if (File.Exists(_filePath))
        {
            await using var stream = File.OpenRead(_filePath);
            loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _jsonOptions, ct);
        }

        Current = MergeWithDefaults(loaded);
        return Current;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        // When saving, ensure that SourceRoot is included in the watched list so that it will be monitored next run.
        if (!string.IsNullOrWhiteSpace(settings.SourceRoot) &&
            !settings.WatchedRoots.Contains(settings.SourceRoot, StringComparer.OrdinalIgnoreCase))
        {
            settings.WatchedRoots.Add(settings.SourceRoot);
        }

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, ct);
        await stream.FlushAsync(ct);

        Current = MergeWithDefaults(settings);
    }

    /// <summary>
    /// Merges a potentially partially populated settings instance with the default
    /// values defined on <see cref="AppSettings"/>.
    /// </summary>
    private static AppSettings MergeWithDefaults(AppSettings? loaded)
    {
        var defaults = new AppSettings();
        if (loaded == null)
            return defaults;

        loaded.WatchedRoots ??= defaults.WatchedRoots;
        loaded.Theme ??= defaults.Theme;
        // For value types we rely on their defaults if they have not been set.
        return loaded;
    }
}
