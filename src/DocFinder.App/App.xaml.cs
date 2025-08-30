using System.IO;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using DocFinder.UI.Views;
using DocFinder.UI.ViewModels;
using DocFinder.Search;
using DocFinder.Catalog;
using DocFinder.Indexing;

namespace DocFinder;

public partial class App
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            services.AddSingleton<ITrayService, TrayService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISearchService, LuceneSearchService>();
            services.AddSingleton<CatalogRepository>();
            services.AddSingleton<IIndexer, DocumentIndexer>();
            services.AddSingleton<SearchOverlayViewModel>();
            services.AddSingleton<SearchOverlay>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsWindow>();
        }).Build();

    public static IServiceProvider Services => _host.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
    }
}
