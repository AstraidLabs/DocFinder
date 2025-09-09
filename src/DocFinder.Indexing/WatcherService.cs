using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DocFinder.Indexing;

public sealed class WatcherService : IWatcherService
{
    private IEnumerable<string> _roots;
    private readonly IIndexer _indexer;
    private readonly ILogger<WatcherService> _logger;
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>();
    private readonly HashSet<string> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;

    public WatcherService(IEnumerable<string> roots, IIndexer indexer, ILogger<WatcherService> logger)
    {
        _roots = roots;
        _indexer = indexer;
        _logger = logger;
        _worker = Task.Run(ProcessQueueAsync);
    }

    public void Start()
    {
        foreach (var root in _roots)
        {
            if (!Directory.Exists(root)) continue;
            var watcher = new FileSystemWatcher(root)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            watcher.Created += OnFileEvent;
            watcher.Changed += OnFileEvent;
            watcher.Deleted += OnFileEvent;
            watcher.Renamed += OnRenamed;
            _watchers.Add(watcher);
        }
    }

    public void UpdateRoots(IEnumerable<string> roots)
    {
        _roots = roots;
        Restart();
    }

    private void Restart()
    {
        Stop();
        Start();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        if (_pending.Add(e.FullPath))
            _queue.Writer.TryWrite(e.FullPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (_pending.Add(e.FullPath))
            _queue.Writer.TryWrite(e.FullPath);
    }

    public void Stop()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            await foreach (var path in _queue.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await _indexer.IndexFileAsync(path, _cts.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index {Path}", path);
                }
                finally
                {
                    _pending.Remove(path);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        Stop();
        _queue.Writer.TryComplete();
        _cts.Cancel();
        try
        {
            await _worker;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Watcher worker failed during dispose");
        }
        _cts.Dispose();
    }
}
