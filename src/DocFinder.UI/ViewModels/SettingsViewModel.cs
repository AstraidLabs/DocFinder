using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using DocFinder.Indexing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;
using System;

namespace DocFinder.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IWatcherService _watcherService;
    private readonly IIndexer _indexer;

    [ObservableProperty]
    private AppSettings _settings = new();

    /// <summary>List of files indexed from the selected source root.</summary>
    public ObservableCollection<string> IndexedFiles { get; } = new();

    public SettingsViewModel(ISettingsService settingsService, IWatcherService watcherService, IIndexer indexer)
    {
        _settingsService = settingsService;
        _watcherService = watcherService;
        _indexer = indexer;
        _settings = settingsService.Current;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        await _settingsService.SaveAsync(Settings, ct);

        // Restart file watchers to reflect the updated roots
        _watcherService.UpdateRoots(Settings.WatchedRoots);

        // Apply the selected theme immediately
        var theme = Settings.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme);

        // Enumerate and index files from the configured source root concurrently
        IndexedFiles.Clear();
        var enumerateTask = Task.Run(() =>
        {
            var files = new List<string>();
            if (!string.IsNullOrWhiteSpace(Settings.SourceRoot) && Directory.Exists(Settings.SourceRoot))
            {
                foreach (var file in Directory.EnumerateFiles(Settings.SourceRoot, "*.*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    files.Add(file);
                }
            }
            return files;
        }, ct);

        var reindexTask = Task.Run(() => _indexer.ReindexAllAsync(ct), ct).Unwrap();

        var filesToIndex = await enumerateTask;
        foreach (var file in filesToIndex)
        {
            ct.ThrowIfCancellationRequested();
            IndexedFiles.Add(file);
        }

        await reindexTask;
    }
}
