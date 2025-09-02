using System;

namespace DocFinder.Domain;

/// <summary>
/// Represents a file stored in the catalog. This entity encapsulates basic validation
/// and exposes behaviour to mutate the file in a controlled manner.
/// </summary>
public class File
{
    private File() { }

    public File(
        Guid fileId,
        string filePath,
        string name,
        string ext,
        long sizeBytes,
        DateTime createdUtc,
        DateTime modifiedUtc,
        string sha256,
        string author,
        Data data)
    {
        FileId = fileId;
        UpdatePath(filePath);
        UpdateName(name);
        SetExt(ext);
        SetAuthor(author);
        UpdateSize(sizeBytes);
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
        UpdateSha256(sha256);
        SetData(data);
    }

    public Guid FileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Ext { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime ModifiedUtc { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;

    public Data Data { get; private set; } = null!;

    public void UpdateName(string name) => Name = ValidateRequired(name, nameof(name));
    public void SetExt(string ext) => Ext = ValidateRequired(ext, nameof(ext));
    public void SetAuthor(string author) => Author = author ?? string.Empty;
    public void UpdateSize(long sizeBytes)
    {
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        SizeBytes = sizeBytes;
    }
    public void UpdateCreated(DateTime createdUtc) => CreatedUtc = createdUtc;
    public void UpdateModified(DateTime modifiedUtc) => ModifiedUtc = modifiedUtc;
    public void UpdateSha256(string sha256) => Sha256 = ValidateRequired(sha256, nameof(sha256));
    public void UpdatePath(string path) => FilePath = ValidateRequired(path, nameof(path));
    public void SetData(Data data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        data.AttachFile(this);
    }

    private static string ValidateRequired(string value, string param)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{param} required", param);
        return value;
    }
}
