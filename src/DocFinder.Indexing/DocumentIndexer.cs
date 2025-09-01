using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Search;
using DocFinder.Catalog;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;

namespace DocFinder.Indexing;

public sealed class DocumentIndexer : IIndexer
{
    private readonly ISearchService _search;
    private readonly CatalogRepository _catalog;
    private readonly ISettingsService _settings;
    private IndexingState _state = IndexingState.Indexing;

    public DocumentIndexer(ISearchService search, CatalogRepository catalog, ISettingsService settings)
    {
        _search = search;
        _catalog = catalog;
        _settings = settings;
    }

    public async Task IndexFileAsync(string path, CancellationToken ct = default)
    {
        if (_state == IndexingState.Paused)
            return;

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            return;

        var ext = fileInfo.Extension.Trim('.').ToLowerInvariant();

        string content = string.Empty;
        string? author = null;
        string? version = null;
        DateTime created = fileInfo.CreationTimeUtc;
        DateTime modified = fileInfo.LastWriteTimeUtc;

        (string content, string? author, string? version, DateTimeOffset? created, DateTimeOffset? modified) meta;
        switch (ext)
        {
            case "pdf":
                meta = await ExtractPdfAsync(path, ct);
                break;
            case "docx":
                meta = await ExtractDocxAsync(path, ct);
                break;
            default:
                meta = (string.Empty, null, null, null, null);
                break;
        }
        content = meta.content;
        author = meta.author;
        version = meta.version;
        if (meta.created.HasValue) created = meta.created.Value.UtcDateTime;
        if (meta.modified.HasValue) modified = meta.modified.Value.UtcDateTime;

        var sha = ComputeSha256(path);
        var fileId = ComputeFileId(path, sha);
        var metaDict = new Dictionary<string,string>();
        if (!string.IsNullOrEmpty(author)) metaDict["author"] = author;
        if (!string.IsNullOrEmpty(version)) metaDict["version"] = version;

        var doc = new IndexDocument(
            fileId,
            path,
            fileInfo.Name,
            ext,
            fileInfo.Length,
            created,
            modified,
            sha,
            author,
            version,
            content,
            metaDict);
        await _search.IndexAsync(doc, ct);
        await _catalog.UpsertFileAsync(doc, ct);
    }

    public async Task ReindexAllAsync()
    {
        foreach (var root in _settings.Current.WatchedRoots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).Trim('.').ToLowerInvariant();
                if (ext is "pdf" or "docx")
                {
                    await IndexFileAsync(file);
                }
            }
        }
    }

    public void Pause() => _state = IndexingState.Paused;

    public void Resume() => _state = IndexingState.Indexing;

    public IndexingState State => _state;

    private static async Task<(string content, string? author, string? version, DateTimeOffset? created, DateTimeOffset? modified)> ExtractPdfAsync(string path, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            using var pdf = PdfDocument.Open(path);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                sb.AppendLine(page.Text);
            }
            var info = pdf.Information;
            DateTimeOffset? created = DateTimeOffset.TryParse(info.CreationDate, out var c) ? c.ToUniversalTime() : null;
            DateTimeOffset? modified = DateTimeOffset.TryParse(info.ModifiedDate, out var m) ? m.ToUniversalTime() : null;
            return (sb.ToString(), info.Author, pdf.Version.ToString(), created, modified);
        }, ct);
    }

    private static async Task<(string content, string? author, string? version, DateTimeOffset? created, DateTimeOffset? modified)> ExtractDocxAsync(string path, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var text = doc.MainDocumentPart?.Document?.InnerText ?? string.Empty;
            var props = doc.PackageProperties;
            DateTimeOffset? created = props.Created.HasValue ? new DateTimeOffset(props.Created.Value).ToUniversalTime() : null;
            DateTimeOffset? modified = props.Modified.HasValue ? new DateTimeOffset(props.Modified.Value).ToUniversalTime() : null;
            return (text, props.Creator, props.Version ?? props.Revision, created, modified);
        }, ct);
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }

    private static Guid ComputeFileId(string path, string sha)
    {
        var bytes = Encoding.UTF8.GetBytes(path + sha);
        var hash = SHA256.HashData(bytes);
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
