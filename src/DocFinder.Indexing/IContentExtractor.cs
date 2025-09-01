namespace DocFinder.Indexing;

public interface IContentExtractor
{
    bool CanHandle(string extension);
    Task<ExtractedContent> ExtractAsync(string path, CancellationToken ct);
}
