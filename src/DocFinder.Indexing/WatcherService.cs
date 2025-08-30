using System;
using System.Collections.Generic;
using System.IO;

namespace DocFinder.Indexing;

public sealed class WatcherService : IDisposable
{
    private readonly IEnumerable<string> _roots;
    private readonly IIndexer _indexer;
    private readonly List<FileSystemWatcher> _watchers = new();

    public WatcherService(IEnumerable<string> roots, IIndexer indexer)
    {
        _roots = roots;
        _indexer = indexer;
    }

    public void Start()
    {
        foreach (var root in _roots)
        {
            if (!Directory.Exists(root)) continue;
            var watcher = new FileSystemWatcher(root)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            _watchers.Add(watcher);
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (File.Exists(e.FullPath))
        {
            _ = _indexer.IndexFileAsync(e.FullPath);
        }
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}
