using System;
using System.Collections.Generic;
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
    {
        var filters = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(command.Filter.FileType) && !string.Equals(command.Filter.FileType, "all", StringComparison.OrdinalIgnoreCase))
            filters["type"] = command.Filter.FileType.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(command.Filter.Author))
            filters["author"] = command.Filter.Author;
        if (!string.IsNullOrWhiteSpace(command.Filter.Version))
            filters["version"] = command.Filter.Version;

        var query = new UserQuery(command.Query)
        {
            Filters = filters,
            FromUtc = command.Filter.FromDate.HasValue ? new DateTimeOffset(command.Filter.FromDate.Value.ToUniversalTime()) : null,
            ToUtc = command.Filter.ToDate.HasValue ? new DateTimeOffset(command.Filter.ToDate.Value.ToUniversalTime()) : null
        };

        return await _search.QueryAsync(query, ct);
    }
}
