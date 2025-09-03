using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Domain.Settings;

namespace DocFinder.Application.Handlers;

public sealed class SearchDocumentsHandler : ICommandHandler<SearchDocumentsCommand, SearchResult>
{
    private readonly ISearchService _search;
    private readonly ISettingsService _settings;
    public SearchDocumentsHandler(ISearchService search, ISettingsService settings)
    {
        _search = search;
        _settings = settings;
    }

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
            ToUtc = command.Filter.ToDate.HasValue ? new DateTimeOffset(command.Filter.ToDate.Value.ToUniversalTime()) : null,
            PageSize = _settings.Current.PageSize
        };

        return await _search.QueryAsync(query, ct);
    }
}
