using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;
using DocFinder.Search;
using DocFinder.Catalog;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;

namespace DocFinder.Indexing;

public sealed class DocumentIndexer
{
    private readonly ISearchService _search;
    private readonly CatalogRepository _catalog;

    public DocumentIndexer(ISearchService search, CatalogRepository catalog)
    {
        _search = search;
        _catalog = catalog;
    }

    public async Task IndexFileAsync(string path, CancellationToken ct = default)
    {
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

        var doc = new IndexDocument(Guid.NewGuid(), path, fileInfo.Name, ext, content, new Dictionary<string,string>(), fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
        await _search.IndexAsync(doc, ct);
        await _catalog.UpsertFileAsync(doc, ct);
    }

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
}
