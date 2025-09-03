using Microsoft.Extensions.Hosting;
using DocFinder.Services;
using DocFinder.App.Views.Windows;
using DocFinder.Indexing;
using DocFinder.Domain.Settings;

namespace DocFinder.App.Services;

public class ApplicationHostService : IHostedService
{
    private readonly ITrayService _tray;
    private readonly SearchOverlay _overlay;
    private readonly SettingsWindow _settings;
    private readonly IWatcherService _watcher;
    private readonly ISettingsService _settingsService;
    private readonly IIndexer _indexer;

    public ApplicationHostService(ITrayService tray,
        SearchOverlay overlay,
        SettingsWindow settings,
        IWatcherService watcher,
        ISettingsService settingsService,
        IIndexer indexer)
    {
        _tray = tray;
        _overlay = overlay;
        _settings = settings;
        _watcher = watcher;
        _settingsService = settingsService;
        _indexer = indexer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher.Start();
        _tray.Initialize(ToggleOverlay, () => System.Windows.Application.Current.Shutdown(), ShowSettings);
        if (_settingsService.Current.AutoIndexOnStartup)
        {
            await _indexer.ReindexAllAsync(cancellationToken);
        }
    }

    private void ToggleOverlay()
    {
        if (_overlay.IsVisible) _overlay.Hide();
        else { _overlay.Show(); _overlay.Activate(); }
    }

    private void ShowSettings() => _settings.Show();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tray.Dispose();
        _watcher.Stop();
        return Task.CompletedTask;
    }
}
