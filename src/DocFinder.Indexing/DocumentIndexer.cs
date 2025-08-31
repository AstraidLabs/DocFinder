using System;
using System.Collections.Generic;
using System.IO;
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

        string content;
        string? author = null;
        string? version = null;
        DateTime created = fileInfo.CreationTimeUtc;
        DateTime modified = fileInfo.LastWriteTimeUtc;

        switch (ext)
        {
            case "pdf":
                var pdf = ExtractPdf(path);
                content = pdf.content;
                author = pdf.author;
                version = pdf.version;
                if (pdf.created.HasValue) created = pdf.created.Value;
                if (pdf.modified.HasValue) modified = pdf.modified.Value;
                break;
            case "docx":
                var docx = ExtractDocx(path);
                content = docx.content;
                author = docx.author;
                version = docx.version;
                if (docx.created.HasValue) created = docx.created.Value;
                if (docx.modified.HasValue) modified = docx.modified.Value;
                break;
            default:
                content = string.Empty;
                break;
        }

        var sha = ComputeSha256(path);
        var meta = new Dictionary<string,string>();
        if (!string.IsNullOrEmpty(author)) meta["author"] = author;
        if (!string.IsNullOrEmpty(version)) meta["version"] = version;

        var doc = new IndexDocument(
            Guid.NewGuid(),
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
            meta);
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

    private static (string content, string? author, string? version, DateTime? created, DateTime? modified) ExtractPdf(string path)
    {
        using var pdf = PdfDocument.Open(path);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        var info = pdf.Information;
        DateTime? created = DateTime.TryParse(info.CreationDate, out var c) ? c : null;
        DateTime? modified = DateTime.TryParse(info.ModifiedDate, out var m) ? m : null;
        return (sb.ToString(), info.Author, pdf.Version.ToString(), created, modified);
    }

    private static (string content, string? author, string? version, DateTime? created, DateTime? modified) ExtractDocx(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        var text = doc.MainDocumentPart?.Document?.InnerText ?? string.Empty;
        var props = doc.PackageProperties;
        return (text, props.Creator, props.Version ?? props.Revision, props.Created, props.Modified);
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}
