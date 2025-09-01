using System.Threading;
using System.Threading.Tasks;

namespace DocFinder.Indexing;

public enum IndexingState
{
    Paused,
    Indexing
}

public interface IIndexer
{
    Task IndexFileAsync(string path, CancellationToken ct = default);
    Task ReindexAllAsync(CancellationToken ct = default);
    void Pause();
    void Resume();
    IndexingState State { get; }
}
