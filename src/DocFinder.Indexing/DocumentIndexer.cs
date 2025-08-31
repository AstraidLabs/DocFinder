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
        string content = ext switch
        {
            "pdf" => ExtractPdf(path),
            "docx" => ExtractDocx(path),
            _ => string.Empty
        };

        var sha = ComputeSha256(path);
        var doc = new IndexDocument(
            Guid.NewGuid(),
            path,
            fileInfo.Name,
            ext,
            fileInfo.Length,
            fileInfo.CreationTimeUtc,
            fileInfo.LastWriteTimeUtc,
            sha,
            content,
            new Dictionary<string,string>());
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

    private static string ExtractPdf(string path)
    {
        using var pdf = PdfDocument.Open(path);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    private static string ExtractDocx(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        return doc.MainDocumentPart?.Document?.InnerText ?? string.Empty;
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}
