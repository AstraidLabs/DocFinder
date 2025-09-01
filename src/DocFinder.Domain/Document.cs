using System;
using System.ComponentModel.DataAnnotations;

namespace DocFinder.Domain;

public class Document
{
    private Document() { }

    public Document(
        int id,
        string buildingName,
        string name,
        string author,
        DateTime modifiedAt,
        string version,
        string type,
        DateTime? issuedAt,
        DateTime? validUntil,
        bool canPrint,
        bool isElectronic,
        string fileLink)
    {
        Id = id;
        BuildingName = !string.IsNullOrWhiteSpace(buildingName) ? buildingName : throw new ArgumentException("Building name required", nameof(buildingName));
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name required", nameof(name));
        Author = !string.IsNullOrWhiteSpace(author) ? author : throw new ArgumentException("Author required", nameof(author));
        ModifiedAt = modifiedAt;
        Version = version ?? string.Empty;
        Type = !string.IsNullOrWhiteSpace(type) ? type : throw new ArgumentException("Type required", nameof(type));
        UpdateValidity(issuedAt, validUntil);
        CanPrint = canPrint;
        IsElectronic = isElectronic;
        FileLink = fileLink ?? string.Empty;
    }

    public int Id { get; private set; }

    [Required, StringLength(200)]
    public string BuildingName { get; private set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; private set; } = string.Empty;

    [Required, StringLength(200)]
    public string Author { get; private set; } = string.Empty;

    public DateTime ModifiedAt { get; private set; }

    [StringLength(50)]
    public string Version { get; private set; } = string.Empty;

    [Required, StringLength(50)]
    public string Type { get; private set; } = string.Empty;

    public DateTime? IssuedAt { get; private set; }

    public DateTime? ValidUntil { get; private set; }

    public bool CanPrint { get; private set; }

    public bool IsElectronic { get; private set; }

    [Url]
    public string FileLink { get; private set; } = string.Empty;

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name;
    }

    public void UpdateValidity(DateTime? issuedAt, DateTime? validUntil)
    {
        if (issuedAt.HasValue && validUntil.HasValue && validUntil < issuedAt)
            throw new ArgumentException("ValidUntil must be after IssuedAt", nameof(validUntil));
        IssuedAt = issuedAt;
        ValidUntil = validUntil;
    }
}

public class AuditEntry
{
    private AuditEntry() { }

    public AuditEntry(int documentId, string action, DateTime timestamp, string userName)
    {
        DocumentId = documentId;
        Action = !string.IsNullOrWhiteSpace(action) ? action : throw new ArgumentException("Action required", nameof(action));
        Timestamp = timestamp;
        UserName = !string.IsNullOrWhiteSpace(userName) ? userName : throw new ArgumentException("User name required", nameof(userName));
    }

    public int Id { get; private set; }

    public int DocumentId { get; private set; }

    [Required]
    public string Action { get; private set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; private set; }

    [Required, StringLength(100)]
    public string UserName { get; private set; } = string.Empty;
}

public interface ILuceneIndexService
{
    void IndexDocument(Document doc);
    void DeleteDocument(int docId);
    IEnumerable<Document> Search(string query);
}
