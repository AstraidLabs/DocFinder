using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Domain;

namespace DocFinder.Application.Handlers;

public sealed class SearchDocumentsHandler : ICommandHandler<SearchDocumentsCommand, SearchResult>
{
    private readonly ISearchService _search;
    public SearchDocumentsHandler(ISearchService search) => _search = search;

    public async Task<SearchResult> HandleAsync(SearchDocumentsCommand command, CancellationToken ct)
        => await _search.QueryAsync(command.Query, ct);
}
