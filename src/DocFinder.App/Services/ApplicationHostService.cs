using Microsoft.Extensions.Hosting;
using DocFinder.Services;
using DocFinder.UI.Views;
using DocFinder.Indexing;

namespace DocFinder.Services;

public class ApplicationHostService : IHostedService
{
    private readonly ITrayService _tray;
    private readonly SearchOverlay _overlay;
    private readonly SettingsWindow _settings;
    private readonly IWatcherService _watcher;

    public ApplicationHostService(ITrayService tray, SearchOverlay overlay, SettingsWindow settings, IWatcherService watcher)
    {
        _tray = tray;
        _overlay = overlay;
        _settings = settings;
        _watcher = watcher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher.Start();
        _tray.Initialize(ToggleOverlay, () => Application.Current.Shutdown(), ShowSettings);
        _overlay.Show();
        _overlay.Activate();
        return Task.CompletedTask;
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
