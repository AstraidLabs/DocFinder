using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using DocFinder.Indexing;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;
using System;
using System.Linq;
using Wpf.Ui.Abstractions.Controls;
using System.Windows;

namespace DocFinder.App.ViewModels;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private readonly ISettingsService _settingsService;
    private readonly IWatcherService _watcherService;
    private readonly IIndexer _indexer;

    [ObservableProperty]
    private AppSettings _settings = new();

    /// <summary>Editable list of watched directories represented as newline separated text.</summary>
    [ObservableProperty]
    private string _watchedRootsText = string.Empty;

    /// <summary>List of files indexed from the selected source root.</summary>
    public ObservableCollection<string> IndexedFiles { get; } = new();

    [ObservableProperty]
    private bool _isIndexing;

    public SettingsViewModel(ISettingsService settingsService, IWatcherService watcherService, IIndexer indexer)
    {
        _settingsService = settingsService;
        _watcherService = watcherService;
        _indexer = indexer;
        _settings = settingsService.Current;
        WatchedRootsText = string.Join(Environment.NewLine, _settings.WatchedRoots);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        Settings.WatchedRoots = WatchedRootsText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        await _settingsService.SaveAsync(Settings, ct);

        // Restart file watchers to reflect the updated roots
        _watcherService.UpdateRoots(Settings.WatchedRoots);

        // Apply the selected theme immediately
        var themeName = string.IsNullOrWhiteSpace(Settings.Theme) ? "Light" : Settings.Theme;
        var theme = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme);

        // Enumerate and index files from the configured source root concurrently
        IndexedFiles.Clear();
        IsIndexing = true;

        var progress = new Progress<string>(path =>
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

        // Start reindexing concurrently without wrapping in an extra Task
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

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
