using System.Diagnostics;
using System.IO;
using System.Threading;
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
using Wpf.Ui.DependencyInjection;

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
            services.AddHostedService<PostStartupBackgroundService>();
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
            services.AddNavigationViewPageProvider();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SearchViewModel>();
            services.AddTransient<SearchPage>();
            services.AddSingleton<SettingsViewModel>();
        }).Build();

    public static IServiceProvider Services => _host.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var loadingWindow = new LoadingWindow();
        loadingWindow.Show();
        loadingWindow.SetProgress(0);
        loadingWindow.SetStatus("Loading settings...");

        try
        {
            var logger = Services.GetRequiredService<ILogger<App>>();
            var settings = Services.GetRequiredService<ISettingsService>();

            await Task.Run(async () =>
            {
                logger.LogInformation("Starting settings load");
                var settingsSw = Stopwatch.StartNew();
                using var settingsCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await settings.LoadAsync(settingsCts.Token).ConfigureAwait(false);
                settingsSw.Stop();
                logger.LogInformation("Settings loaded in {Elapsed} ms", settingsSw.ElapsedMilliseconds);
                loadingWindow.SetProgress(33);
                loadingWindow.SetStatus("Starting host...");

                //MIGRACE – před startem hosta, v samostatném scope, bez dalších otevřených připojení
                logger.LogInformation("Applying database migrations");
                var migrateSw = Stopwatch.StartNew();
                using (var scope = Services.CreateScope())
                {
                    var ctxFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DocumentDbContext>>();
                    await using var ctx = await ctxFactory.CreateDbContextAsync();

                    // (volitelné, ale pro SQLite doporučené – sníží kolize zámků)
                    await ctx.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
                    await ctx.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;");

                    using var migrateCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    await ctx.Database.MigrateAsync(migrateCts.Token);
                }
                migrateSw.Stop();
                logger.LogInformation("Migrations applied in {Elapsed} ms", migrateSw.ElapsedMilliseconds);
                loadingWindow.SetProgress(50);

                // ❷ Teprve teď startuj hosta
                logger.LogInformation("Starting host");
                var hostSw = Stopwatch.StartNew();
                using var hostCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await _host.StartAsync(hostCts.Token).ConfigureAwait(false);
                hostSw.Stop();
                logger.LogInformation("Host started in {Elapsed} ms", hostSw.ElapsedMilliseconds);
                loadingWindow.SetProgress(66);
            }).ConfigureAwait(true);

            // Apply the configured theme so that the window uses it immediately
            var themeName = settings.Current.Theme;
            var theme = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
            ApplicationThemeManager.Apply(theme);

            loadingWindow.SetStatus("Initializing UI...");
            var uiSw = Stopwatch.StartNew();
            var navigation = Services.GetRequiredService<INavigationService>();
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.SetServiceProvider(Services);
            navigation.SetNavigationControl(mainWindow.GetNavigation());
            navigation.Navigate(typeof(SearchPage));
            mainWindow.Show();
            uiSw.Stop();
            logger.LogInformation("UI initialized in {Elapsed} ms", uiSw.ElapsedMilliseconds);
            loadingWindow.SetProgress(100);
        }
        catch (Exception ex)
        {
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogError(ex, "Failed to start application");

            var dialog = Services.GetRequiredService<IMessageDialogService>();
            await dialog.ShowError($"Failed to start application: {ex.Message}", "Startup Error");

            Shutdown();
        }
        finally
        {
            loadingWindow.Close();
        }
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        try
        {
            // Persist user changes.  Like .NET user scoped settings, values are only
            // written when SaveAsync is called explicitly.
            var settings = Services.GetRequiredService<ISettingsService>();
            await settings.SaveAsync(settings.Current);

            await _host.StopAsync();
        }
        catch (Exception ex)
        {
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogError(ex, "Error during application shutdown");
        }
        finally
        {
            _host.Dispose();
        }
    }

    private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogError(e.Exception, "Unhandled exception");

        try
        {
            var dialog = Services.GetRequiredService<IMessageDialogService>();
            var continueApp = await dialog.ShowConfirmation(
                $"An unexpected error occurred:\n{e.Exception.Message}\n\nDo you want to continue using the application?",
                "Unexpected Error");

            e.Handled = continueApp;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process unhandled exception");
            e.Handled = false;
        }
    }
}
