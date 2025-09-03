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
using DocFinder.UI.Views;
using DocFinder.UI.ViewModels;
using DocFinder.UI.Services;
using DocFinder.Search;
using DocFinder.Catalog;
using DocFinder.Indexing;
using DocFinder.Application;
using DocFinder.Application.Commands;
using DocFinder.Application.Handlers;

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
            services.AddSingleton<SearchOverlayViewModel>();
            services.AddSingleton<SearchOverlay>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsWindow>();
        }).Build();

    public static IServiceProvider Services => _host.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // Load user settings before any services are started so that other
        // services (like the file watcher) receive the correct configuration.
        var settings = Services.GetRequiredService<ISettingsService>();
        await settings.LoadAsync();

        await _host.StartAsync();

        var overlay = Services.GetRequiredService<SearchOverlay>();
        overlay.Show();
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
