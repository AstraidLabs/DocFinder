using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DocFinder.Domain.Settings;
using DocFinder.Indexing;
using DocFinder.App.Services;
using Wpf.Ui.Appearance;

namespace DocFinder.App.ViewModels;

/// <summary>
/// View model backing the application settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IWatcherService _watcherService;
    private readonly IIndexer _indexer;
    private readonly IThemeService _themeService;

    /// <summary>Current application settings.</summary>
    [ObservableProperty]
    private AppSettings _settings = new();

    /// <summary>Editable list of watched directories represented as newline separated text.</summary>
    [ObservableProperty]
    private string _watchedRootsText = string.Empty;

    /// <summary>List of files indexed from the selected source root.</summary>
    public ObservableCollection<string> IndexedFiles { get; } = new();

    [ObservableProperty]
    private bool _isIndexing;

    public SettingsViewModel(
        ISettingsService settingsService,
        IWatcherService watcherService,
        IIndexer indexer,
        IThemeService themeService)
    {
        _settingsService = settingsService;
        _watcherService = watcherService;
        _indexer = indexer;
        _themeService = themeService;

        Settings = settingsService.Current;
        WatchedRootsText = string.Join(Environment.NewLine, Settings.WatchedRoots);
    }

    /// <summary>Persist settings and restart indexing.</summary>
    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        Settings.WatchedRoots = WatchedRootsText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        await _settingsService.SaveAsync(Settings, ct);

        _watcherService.UpdateRoots(Settings.WatchedRoots);

        ApplyTheme(Settings.Theme);

        IndexedFiles.Clear();
        IsIndexing = true;

        IProgress<string> progress = new Progress<string>(path =>
        {
            Application.Current.Dispatcher.InvokeAsync(() => IndexedFiles.Add(path));
        });

        var enumerateTask = Task.Run(() =>
        {
            if (!string.IsNullOrWhiteSpace(Settings.SourceRoot) && Directory.Exists(Settings.SourceRoot))
            {
                foreach (var file in Directory.EnumerateFiles(Settings.SourceRoot, "*.*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(file);
                }
            }
        }, ct);

        var reindexTask = _indexer.ReindexAllAsync(ct);

        try
        {
            await Task.WhenAll(enumerateTask, reindexTask);
        }
        finally
        {
            IsIndexing = false;
        }
    }

    /// <summary>Switch to light theme.</summary>
    [RelayCommand]
    private void SetLightTheme()
    {
        ApplyTheme("Light");
    }

    /// <summary>Switch to dark theme.</summary>
    [RelayCommand]
    private void SetDarkTheme()
    {
        ApplyTheme("Dark");
    }

    /// <summary>Switch theme based on system settings.</summary>
    [RelayCommand]
    private void SetAutoTheme()
    {
        ApplyTheme("Auto");
    }

    private void ApplyTheme(string? themeName)
    {
        Settings.Theme = themeName;
        if (themeName?.Equals("Dark", StringComparison.OrdinalIgnoreCase) == true)
        {
            _themeService.SetTheme(ApplicationTheme.Dark);
        }
        else if (themeName?.Equals("Auto", StringComparison.OrdinalIgnoreCase) == true)
        {
            ApplicationThemeManager.ApplySystemTheme();
        }
        else
        {
            _themeService.SetTheme(ApplicationTheme.Light);
        }
    }
}

