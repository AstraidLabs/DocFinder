using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DocFinder.App.Views.Windows;

namespace DocFinder.App.Services;

public class PostStartupBackgroundService : BackgroundService
{
    private readonly MainWindow _mainWindow;
    private readonly ILogger<PostStartupBackgroundService> _logger;

    public PostStartupBackgroundService(MainWindow mainWindow, ILogger<PostStartupBackgroundService> logger)
    {
        _mainWindow = mainWindow;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_mainWindow.IsLoaded)
        {
            var tcs = new TaskCompletionSource();
            void LoadedHandler(object? sender, RoutedEventArgs e)
            {
                _mainWindow.Loaded -= LoadedHandler;
                tcs.SetResult();
            }
            _mainWindow.Loaded += LoadedHandler;
            await tcs.Task;
        }

        _logger.LogInformation("Post-startup background service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
