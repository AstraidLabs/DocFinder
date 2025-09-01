using DocumentFormat.OpenXml.Packaging;

namespace DocFinder.Indexing;

public sealed class DocxContentExtractor : IContentExtractor
{
    public bool CanHandle(string extension) => extension.Equals("docx", StringComparison.OrdinalIgnoreCase);

    public Task<ExtractedContent> ExtractAsync(string path, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var text = doc.MainDocumentPart?.Document?.InnerText ?? string.Empty;
            var props = doc.PackageProperties;
            DateTimeOffset? created = props.Created.HasValue ? new DateTimeOffset(props.Created.Value).ToUniversalTime() : null;
            DateTimeOffset? modified = props.Modified.HasValue ? new DateTimeOffset(props.Modified.Value).ToUniversalTime() : null;
            var version = props.Version ?? props.Revision;
            return new ExtractedContent(text, props.Creator, version, created, modified);
        }, ct);
    }
}
