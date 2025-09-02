using System;

namespace DocFinder.Domain;

public class File
{
    public Guid FileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Ext { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public Data Data { get; set; } = null!;
}
