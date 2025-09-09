using Microsoft.Extensions.Hosting;
using DocFinder.Services;
using DocFinder.App.Views.Windows;
using DocFinder.App.Views.Pages;
using DocFinder.Indexing;
using DocFinder.Domain.Settings;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ApplicationHostService> _logger;

    public ApplicationHostService(ITrayService tray,
        MainWindow mainWindow,
        INavigationService navigationService,
        IWatcherService watcher,
        ISettingsService settingsService,
        IIndexer indexer,
        ILogger<ApplicationHostService> logger)
    {
        _tray = tray;
        _mainWindow = mainWindow;
        _navigationService = navigationService;
        _watcher = watcher;
        _settingsService = settingsService;
        _indexer = indexer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher.Start();
        _tray.Initialize(ToggleMainWindow, () => System.Windows.Application.Current.Shutdown(), ShowSettings);
        if (_settingsService.Current.AutoIndexOnStartup)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000);
                    _logger.LogInformation("Starting initial reindexation");
                    using var reindexCts = new CancellationTokenSource();
                    await _indexer.ReindexAllAsync(reindexCts.Token);
                    _logger.LogInformation("Initial reindexation completed");
                    _tray.ShowNotification("DocFinder", "Initial indexation completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Initial reindexation failed");
                    _tray.ShowNotification("DocFinder", "Initial indexation failed");
                }
            });
        }

        return Task.CompletedTask;
    }

    private void ToggleMainWindow()
    {
        if (_mainWindow.IsVisible) _mainWindow.Hide();
        else { _mainWindow.Show(); _mainWindow.Activate(); }
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
