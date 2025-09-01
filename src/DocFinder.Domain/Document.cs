using System;

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
        string? version,
        string type,
        DateTime? issuedAt,
        DateTime? validUntil,
        bool canPrint,
        bool isElectronic,
        string? fileLink)
    {
        Id = id;
        SetBuildingName(buildingName);
        UpdateName(name);
        SetAuthor(author);
        ModifiedAt = modifiedAt;
        SetVersion(version);
        SetType(type);
        UpdateValidity(issuedAt, validUntil);
        CanPrint = canPrint;
        IsElectronic = isElectronic;
        SetFileLink(fileLink);
    }

    public int Id { get; private set; }

    public string BuildingName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public DateTime ModifiedAt { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public DateTime? IssuedAt { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public bool CanPrint { get; private set; }
    public bool IsElectronic { get; private set; }
    public string FileLink { get; private set; } = string.Empty;

    public void UpdateName(string name) => Name = ValidateRequired(name, nameof(name));

    public void UpdateValidity(DateTime? issuedAt, DateTime? validUntil)
    {
        if (issuedAt.HasValue && validUntil.HasValue && validUntil < issuedAt)
            throw new ArgumentException("ValidUntil must be after IssuedAt", nameof(validUntil));
        IssuedAt = issuedAt;
        ValidUntil = validUntil;
    }

    private void SetBuildingName(string name) => BuildingName = ValidateRequired(name, nameof(name));
    private void SetAuthor(string author) => Author = ValidateRequired(author, nameof(author));
    private void SetVersion(string? version) => Version = version ?? string.Empty;
    private void SetType(string type) => Type = ValidateRequired(type, nameof(type));
    private void SetFileLink(string? link) => FileLink = link ?? string.Empty;

    private static string ValidateRequired(string value, string param)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{param} required", param);
        return value;
    }
}
