using System;

namespace DocFinder.Domain;

public class Metadata
{
    public Guid FileId { get; set; }
    public File File { get; set; } = null!;
    public string? Version { get; set; }
    public string? CaseNumber { get; set; }
    public string? ParcelId { get; set; }
    public string? Address { get; set; }
    public string? Tags { get; set; }
}
