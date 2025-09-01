using System.Text;
using UglyToad.PdfPig;

namespace DocFinder.Indexing;

public sealed class PdfContentExtractor : IContentExtractor
{
    public bool CanHandle(string extension) => extension.Equals("pdf", StringComparison.OrdinalIgnoreCase);

    public Task<ExtractedContent> ExtractAsync(string path, CancellationToken ct)
    {
        return Task.Run(() =>
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
            return new ExtractedContent(sb.ToString(), info.Author, pdf.Version.ToString(), created, modified);
        }, ct);
    }
}
