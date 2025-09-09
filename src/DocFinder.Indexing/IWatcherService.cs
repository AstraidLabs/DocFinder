using System;
using System.Collections.Generic;

namespace DocFinder.Indexing;

public interface IWatcherService : IAsyncDisposable
{
    void Start();
    void Stop();
    /// <summary>Reconfigure the watcher to monitor a new set of roots.</summary>
    void UpdateRoots(IEnumerable<string> roots);
}

