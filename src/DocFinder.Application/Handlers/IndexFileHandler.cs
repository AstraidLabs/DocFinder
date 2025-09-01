using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Indexing;

namespace DocFinder.Application.Handlers;

public sealed class IndexFileHandler : ICommandHandler<IndexFileCommand, Unit>
{
    private readonly IIndexer _indexer;
    public IndexFileHandler(IIndexer indexer) => _indexer = indexer;

    public async Task<Unit> HandleAsync(IndexFileCommand command, CancellationToken ct)
    {
        await _indexer.IndexFileAsync(command.Path, ct);
        return new Unit();
    }
}
