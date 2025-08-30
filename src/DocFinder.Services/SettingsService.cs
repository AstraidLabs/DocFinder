using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain.Settings;

namespace DocFinder.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IHotkeyService _hotkeyService;
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public SettingsService(IHotkeyService hotkeyService, string? filePath = null)
    {
        _hotkeyService = hotkeyService;
        _filePath = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "DocFinder", "settings.json");
    }

    public AppSettings Current { get; private set; } = new();

    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (File.Exists(_filePath))
        {
            await using var stream = File.OpenRead(_filePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _jsonOptions, ct);
            Current = settings ?? new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
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

        Current = settings;
        await _hotkeyService.UnregisterAsync(ct);
        await _hotkeyService.RegisterAsync(settings.GlobalHotkey, ct);
    }
}
