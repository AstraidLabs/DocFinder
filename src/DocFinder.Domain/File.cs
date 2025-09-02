using System;

namespace DocFinder.Domain;

public class File
{
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

    private File() { }

    public File(Guid fileId, string name, string ext, long sizeBytes, DateTime createdUtc, DateTime modifiedUtc, string sha256, string filePath, string author = "")
    {
        if (fileId == Guid.Empty) throw new ArgumentException("File id is required", nameof(fileId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(ext)) throw new ArgumentException("Extension is required", nameof(ext));
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        if (string.IsNullOrWhiteSpace(sha256)) throw new ArgumentException("Sha256 is required", nameof(sha256));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path is required", nameof(filePath));

        FileId = fileId;
        Name = name;
        Ext = ext;
        SizeBytes = sizeBytes;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
        Sha256 = sha256;
        FilePath = filePath;
        Author = author ?? string.Empty;
    }

    public void Update(string name, string ext, long sizeBytes, DateTime createdUtc, DateTime modifiedUtc, string sha256, string filePath, string author = "")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(ext)) throw new ArgumentException("Extension is required", nameof(ext));
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        if (string.IsNullOrWhiteSpace(sha256)) throw new ArgumentException("Sha256 is required", nameof(sha256));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path is required", nameof(filePath));

        Name = name;
        Ext = ext;
        SizeBytes = sizeBytes;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
        Sha256 = sha256;
        FilePath = filePath;
        Author = author ?? string.Empty;
    }

    public void SetData(Data data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        data.SetFile(this);
    }
}

