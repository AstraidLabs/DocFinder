using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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

public record DocumentRecord(
    Guid FileId,
    string Path,
    string FileName,
    string Ext,
    long SizeBytes,
    DateTime CreatedUtc,
    DateTime ModifiedUtc,
    string Sha256,
    string? Author,
    string? Version,
    string? CaseNumber,
    string? ParcelId,
    string? Address,
    string? Tags);

public sealed record IndexDocument(
    Guid FileId,
    string Path,
    string FileName,
    string Ext,
    long SizeBytes,
    DateTime CreatedUtc,
    DateTime ModifiedUtc,
    string Sha256,
    string? Author,
    string? Version,
    string Content,
    IDictionary<string,string> Metadata,
    string? CaseNumber = null,
    string? ParcelId = null,
    string? Address = null,
    string? Tags = null)
    : DocumentRecord(FileId, Path, FileName, Ext, SizeBytes, CreatedUtc, ModifiedUtc, Sha256, Author, Version, CaseNumber, ParcelId, Address, Tags);

public sealed record SearchHit(
    Guid FileId,
    string FileName,
    string Path,
    string Ext,
    long SizeBytes,
    DateTime CreatedUtc,
    DateTime ModifiedUtc,
    string Sha256,
    string? Author,
    string? Version,
    float Score,
    string? Snippet,
    IReadOnlyDictionary<string,string> Meta)
{
    // Sort key ignoring case and diacritics for alphabetical ordering
    // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-sort-data-in-a-view
    public string SortKey => RemoveDiacritics(FileName).ToLowerInvariant();

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

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
