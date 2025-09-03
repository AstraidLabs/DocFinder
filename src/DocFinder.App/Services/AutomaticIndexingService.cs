using DocFinder.Services;
using Microsoft.Extensions.Hosting;
using DocFinder.Indexing;

namespace DocFinder.App.Services;

public class AutomaticIndexingService : BackgroundService
{
    private readonly IIndexer _indexer;
    private readonly ITrayService _tray;

    public AutomaticIndexingService(IIndexer indexer, ITrayService tray)
    {
        _indexer = indexer;
        _tray = tray;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _indexer.ReindexAllAsync(stoppingToken);
            _tray.ShowNotification("DocFinder", "Automatické indexování dokončeno");
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
