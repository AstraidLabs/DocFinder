using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DocFinder.App.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace DocFinder.App.Services;

public class PostStartupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostStartupBackgroundService> _logger;

    public PostStartupBackgroundService(IServiceProvider serviceProvider, ILogger<PostStartupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            if (!mainWindow.IsLoaded)
            {
                var tcs = new TaskCompletionSource();
                void LoadedHandler(object? sender, RoutedEventArgs e)
                {
                    mainWindow.Loaded -= LoadedHandler;
                    tcs.SetResult();
                }
                mainWindow.Loaded += LoadedHandler;
                await tcs.Task;
            }
        });

        _logger.LogInformation("Post-startup background service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
