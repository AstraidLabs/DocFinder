using System;
using System.Collections.Generic;

namespace DocFinder.Domain;

public class Document
{
    public int Id { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? IssuedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool CanPrint { get; set; }
    public bool IsElectronic { get; set; }
    public string FileLink { get; set; } = string.Empty;
}

public class AuditEntry
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public interface ILuceneIndexService
{
    void IndexDocument(Document doc);
    void DeleteDocument(int docId);
    IEnumerable<Document> Search(string query);
}
