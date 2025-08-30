using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocFinder.Domain;

public sealed record UserQuery(
    string FreeText,
    bool UseFuzzy,
    IReadOnlyDictionary<string,string>? Filters,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page = 1,
    int PageSize = 20,
    string? Sort = null);

public sealed record IndexDocument(
    Guid FileId,
    string Path,
    string FileName,
    string Ext,
    string Content,
    IDictionary<string,string> Metadata,
    DateTime CreatedUtc,
    DateTime ModifiedUtc);

public sealed record SearchHit(
    Guid FileId,
    string FileName,
    string Path,
    string Ext,
    DateTime ModifiedUtc,
    float Score,
    string? Snippet,
    IReadOnlyDictionary<string,string> Meta);

public sealed record SearchResult(
    int Total,
    IReadOnlyList<SearchHit> Hits,
    IReadOnlyDictionary<string,int> Facets);

public interface ISearchService
{
    Task IndexAsync(IndexDocument doc, CancellationToken ct = default);
    Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct = default);
    Task<SearchResult> QueryAsync(UserQuery query, CancellationToken ct = default);
    Task OptimizeAsync(CancellationToken ct = default);
}
