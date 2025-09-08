using System.IO;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using DocFinder.App.Services;
using DocFinder.App.Views.Windows;
using DocFinder.App.Views.Pages;
using DocFinder.App.ViewModels.Pages;
using DocFinder.App.ViewModels.Windows;
using DocFinder.App.ViewModels;
using DocFinder.Search;
using DocFinder.Catalog;
using DocFinder.Indexing;
using DocFinder.Application;
using DocFinder.Application.Commands;
using DocFinder.Application.Handlers;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace DocFinder.App;

public partial class App
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            services.AddHostedService<AutomaticIndexingService>();
            services.AddSingleton<ITrayService, TrayService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISearchService, LuceneSearchService>();
            services.AddSingleton<ILuceneIndexService, LuceneIndexService>();
            services.AddScoped<DocumentSaveChangesInterceptor>();
            services.AddDbContextFactory<DocumentDbContext>(o =>
                o.UseSqlite(DocumentDbContext.DefaultConnectionString));
            services.AddDbContext<DocumentDbContext>((sp, o) =>
            {
                o.UseSqlite(DocumentDbContext.DefaultConnectionString);
                o.AddInterceptors(sp.GetRequiredService<DocumentSaveChangesInterceptor>());
            });
            services.AddSingleton<IDocumentIndexService, DocumentIndexService>();
            services.AddSingleton<CatalogRepository>();
            services.AddSingleton<IContentExtractor, PdfContentExtractor>();
            services.AddSingleton<IContentExtractor, DocxContentExtractor>();
            services.AddSingleton<IIndexer, DocumentIndexer>();
            services.AddSingleton<CommandDispatcher>();
            services.AddTransient<ICommandHandler<SearchDocumentsCommand, SearchResult>, SearchDocumentsHandler>();
            services.AddTransient<ICommandHandler<IndexFileCommand, Unit>, IndexFileHandler>();
            services.AddTransient<ICommandHandler<DeleteDocumentCommand, Unit>, DeleteDocumentHandler>();
            services.AddSingleton<IWatcherService>(sp =>
                new WatcherService(
                    sp.GetRequiredService<ISettingsService>().Current.WatchedRoots,
                    sp.GetRequiredService<IIndexer>(),
                    sp.GetRequiredService<ILogger<WatcherService>>()));
            services.AddSingleton<IDocumentViewService, DocumentViewService>();
            services.AddSingleton<IMessageDialogService, MessageDialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INavigationViewPageProvider, NavigationViewPageProvider>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SearchViewModel>();
            services.AddTransient<SearchPage>();
            services.AddSingleton<SettingsViewModel>();
        }).Build();

    public static IServiceProvider Services => _host.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // Load user settings before any services are started so that other
        // services (like the file watcher) receive the correct configuration.
        var settings = Services.GetRequiredService<ISettingsService>();
        await settings.LoadAsync();

        // Apply the configured theme so that the loading window uses it immediately
        var themeName = settings.Current.Theme;
        var theme = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme);

        var loadingWindow = new LoadingWindow();
        loadingWindow.Show();

        await _host.StartAsync();

        loadingWindow.Close();

        var navigation = Services.GetRequiredService<INavigationService>();
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.SetServiceProvider(Services);
        navigation.SetNavigationControl(mainWindow.GetNavigation());
        navigation.Navigate(typeof(SearchPage));
        mainWindow.Show();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        // Persist user changes.  Like .NET user scoped settings, values are only
        // written when SaveAsync is called explicitly.
        var settings = Services.GetRequiredService<ISettingsService>();
        await settings.SaveAsync(settings.Current);

        await _host.StopAsync();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
    }
}
