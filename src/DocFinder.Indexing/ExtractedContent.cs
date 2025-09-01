namespace DocFinder.Indexing;

public record ExtractedContent(
    string Content,
    string? Author,
    string? Version,
    DateTimeOffset? Created,
    DateTimeOffset? Modified);
