using System;

namespace DocFinder.Indexing;

public interface IWatcherService : IDisposable
{
    void Start();
}

