using Microsoft.Extensions.Hosting;
using DocFinder.Services;
using DocFinder.App.Views.Windows;
using DocFinder.App.Views.Pages;
using DocFinder.Indexing;
using DocFinder.Domain.Settings;
using Wpf.Ui;

namespace DocFinder.App.Services;

public class ApplicationHostService : IHostedService
{
    private readonly ITrayService _tray;
    private readonly MainWindow _mainWindow;
    private readonly INavigationService _navigationService;
    private readonly IWatcherService _watcher;
    private readonly ISettingsService _settingsService;
    private readonly IIndexer _indexer;

    public ApplicationHostService(ITrayService tray,
        MainWindow mainWindow,
        INavigationService navigationService,
        IWatcherService watcher,
        ISettingsService settingsService,
        IIndexer indexer)
    {
        _tray = tray;
        _mainWindow = mainWindow;
        _navigationService = navigationService;
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
        if (_mainWindow.IsVisible)
            _mainWindow.Hide();
        else
        {
            _mainWindow.Show();
            _mainWindow.Activate();
            _navigationService.Navigate(typeof(SearchPage));
        }
    }

    private void ShowSettings()
    {
        if (!_mainWindow.IsVisible)
            _mainWindow.Show();
        _mainWindow.Activate();
        _navigationService.Navigate(typeof(SettingsPage));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tray.Dispose();
        _watcher.Stop();
        return Task.CompletedTask;
    }
}
